// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using EngineSceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CodeSmile.MultiPal.Scene
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ClientSceneLoader))]
	public sealed class ServerSceneLoader : MonoBehaviour
	{
		private readonly HashSet<SceneReference> m_SynchedScenes = new();

		private Queue<ProcessingScene> m_ScenesProcessing;
		private TaskCompletionSource<Boolean> m_TaskInProgress;

		private ClientSceneLoader m_ClientLoader;

		private Boolean m_IsServerOnline;

		private NetworkManager NetworkManager => NetworkManager.Singleton;
		private NetworkSceneManager SceneManager => NetworkManager.Singleton?.SceneManager;

		public Boolean IsProcessingEvents => m_ScenesProcessing != null;

		private void Awake()
		{
			ComponentsRegistry.Set(this);
			m_ClientLoader = GetComponent<ClientSceneLoader>();
		}

		private void Start() => RegisterCallbacks();

		private void RegisterCallbacks()
		{
			NetworkManager.OnServerStarted += OnServerStarted;
			NetworkManager.OnServerStopped += OnServerStopped;
			NetworkManager.OnClientStarted += OnClientStarted;
			NetworkManager.OnClientStopped += OnClientStopped;
		}

		private void UnregisterCallbacks()
		{
			if (NetworkManager != null)
			{
				NetworkManager.OnServerStarted -= OnServerStarted;
				NetworkManager.OnServerStopped -= OnServerStopped;
				NetworkManager.OnClientStarted -= OnClientStarted;
				NetworkManager.OnClientStopped -= OnClientStopped;
			}
		}

		private void OnServerStarted()
		{
			m_IsServerOnline = true;

			SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
			SceneManager.OnSceneEvent += OnServerSceneEvents;
			SceneManager.VerifySceneBeforeLoading += ServerVerifySceneBeforeLoading;
			SceneManager.VerifySceneBeforeUnloading += ServerVerifySceneBeforeUnloading;
		}

		private void OnServerStopped(Boolean isHost)
		{
			m_IsServerOnline = false;
			if (SceneManager != null)
			{
				SceneManager.OnSceneEvent -= OnServerSceneEvents;
				SceneManager.VerifySceneBeforeLoading -= ServerVerifySceneBeforeLoading;
				SceneManager.VerifySceneBeforeUnloading -= ServerVerifySceneBeforeUnloading;
			}

			StopAllCoroutines();
			StopProcessing();
			LocallyUnloadSynchedScenes();
		}

		private void OnClientStarted()
		{
			if (NetworkManager.IsHost == false)
				SceneManager.OnSceneEvent += OnClientSceneEvents;

			SceneManager.VerifySceneBeforeLoading += ClientVerifySceneBeforeLoading;
			SceneManager.VerifySceneBeforeUnloading += ClientVerifySceneBeforeUnloading;
		}

		private void OnClientStopped(Boolean isHost)
		{
			if (isHost == false)
			{
				if (SceneManager != null)
				{
					SceneManager.OnSceneEvent -= OnClientSceneEvents;
					SceneManager.VerifySceneBeforeLoading -= ClientVerifySceneBeforeLoading;
					SceneManager.VerifySceneBeforeUnloading -= ClientVerifySceneBeforeUnloading;
				}

				LocallyUnloadSynchedScenes();
			}
		}

		private Boolean ServerVerifySceneBeforeUnloading(UnityEngine.SceneManagement.Scene scene) =>
			scene.buildIndex != 0; // never our 'global' scene

		private Boolean ClientVerifySceneBeforeUnloading(UnityEngine.SceneManagement.Scene scene) =>
			scene.buildIndex != 0; // never our 'global' scene

		private Boolean ServerVerifySceneBeforeLoading(Int32 sceneIndex, String sceneName, LoadSceneMode loadMode) =>
			sceneIndex != 0; // never our 'global' scene

		private Boolean ClientVerifySceneBeforeLoading(Int32 sceneIndex, String sceneName, LoadSceneMode loadMode) =>
			sceneIndex != 0; // never our 'global' scene

		private void StopProcessing()
		{
			m_ScenesProcessing = null;

			if (m_TaskInProgress != null)
			{
				m_TaskInProgress.SetCanceled();
				m_TaskInProgress = null;
			}
		}

		/// <summary>
		///     Will unload the network synchronized scenes when either client or server stops
		/// </summary>
		private void LocallyUnloadSynchedScenes()
		{
			if (m_SynchedScenes != null)
			{
				foreach (var sceneReference in m_SynchedScenes)
				{
					var scene = EngineSceneManager.GetSceneByPath(sceneReference.ScenePath);
					if (scene.IsValid() && scene.isLoaded)
					{
						//Debug.Log($"Local unload of: {sceneReference.SceneName}");
						EngineSceneManager.UnloadSceneAsync(sceneReference.ScenePath);
					}
				}

				m_SynchedScenes.Clear();
			}
		}

		public async Task UnloadScenesAsync(SceneReference[] scenesToUnload) =>
			await InternalLoadUnloadScenesAsync(scenesToUnload, SceneOperation.Unload);

		public async Task LoadScenesAsync(SceneReference[] scenesToLoad) =>
			await InternalLoadUnloadScenesAsync(scenesToLoad, SceneOperation.Load);

		private async Task InternalLoadUnloadScenesAsync(SceneReference[] scenes, SceneOperation operation)
		{
			if (m_IsServerOnline == false) // also guards against clients calling into here
				return;

			if (scenes == null || scenes.Length == 0)
				return;

			if (IsProcessingEvents)
			{
				// can't load again while still loading/unloading!
				// Note: while possible to add scenes to ongoing queue, there may be different callers awaiting so we'd force
				// one caller to also wait for the other's scenes! This is unexpected with possible severe side-effects!
				Debug.LogError("Scene load/unload event in progress! Forgot to await??");
				return;
			}

			// task is for awaiting, will get its result when entire queue has been processed
			m_TaskInProgress = new TaskCompletionSource<Boolean>();
			m_ScenesProcessing = new Queue<ProcessingScene>();
			EnqueueScenes(scenes, operation);

			ProcessNextSceneInQueue();

			// await our dummy result until queue is complete
			await m_TaskInProgress.Task;
			m_TaskInProgress = null; // we no longer need you
		}

		private void EnqueueScenes(SceneReference[] scenes, SceneOperation operation)
		{
			foreach (var sceneRef in scenes)
				m_ScenesProcessing.Enqueue(new ProcessingScene { SceneRef = sceneRef, Operation = operation });
		}

		private void InternalLoadQueuedSceneAsync(SceneReference sceneRef)
		{
			if (m_SynchedScenes.Contains(sceneRef))
			{
				//Debug.LogWarning($"skip load, already loaded: {sceneRef.SceneName}");
				ProcessNextSceneInQueue();
				return;
			}

			var status = SceneManager.LoadScene(sceneRef.SceneName, LoadSceneMode.Additive);

			if (status == SceneEventProgressStatus.Started)
				m_SynchedScenes.Add(sceneRef);
			else
			{
				Debug.LogWarning($"Scene load failed: {status} for {sceneRef.ScenePath}");
				ProcessNextSceneInQueue();
			}
		}

		private void InternalUnloadQueuedSceneAsync(SceneReference sceneRef)
		{
			if (m_SynchedScenes.Contains(sceneRef) == false)
			{
				//Debug.LogWarning($"skip unload, not loaded: {sceneRef.SceneName}");
				ProcessNextSceneInQueue();
				return;
			}

			var status = SceneManager.UnloadScene(sceneRef.RuntimeScene);

			if (status == SceneEventProgressStatus.Started)
				m_SynchedScenes.Remove(sceneRef);
			else
			{
				Debug.LogWarning($"Scene unload failed: {status} for {sceneRef.ScenePath}");
				ProcessNextSceneInQueue();
			}
		}

		private void ProcessNextSceneInQueue()
		{
			if (m_ScenesProcessing.Count > 0)
			{
				// process the next scene
				var processingScene = m_ScenesProcessing.Dequeue();
				if (processingScene.Operation == SceneOperation.Load)
					InternalLoadQueuedSceneAsync(processingScene.SceneRef);
				else
					InternalUnloadQueuedSceneAsync(processingScene.SceneRef);
			}
			else
			{
				// end processing, SetResult stops awaiting
				m_ScenesProcessing = null;
				m_TaskInProgress.SetResult(true);
			}
		}

		private void OnServerSceneEvents(SceneEvent sceneEvent)
		{
			switch (sceneEvent.SceneEventType)
			{
				case SceneEventType.Load:
				case SceneEventType.Unload:
					sceneEvent.AsyncOperation.completed += OnServerSceneEventComplete;
					break;
			}
		}

		private void OnServerSceneEventComplete(AsyncOperation asyncOp)
		{
			asyncOp.completed -= OnServerSceneEventComplete;

			ProcessNextSceneInQueue();
		}

		private void OnClientSceneEvents(SceneEvent sceneEvent)
		{
			// if we have the scene loaded client-side it cannot be a network synched scene!
			if (m_ClientLoader.IsSceneLoaded(sceneEvent.SceneName))
				return;

			// add or remove the scene from the synched scenes list
			// we need these on client-side to unload them when client leaves
			switch (sceneEvent.SceneEventType)
			{
				case SceneEventType.LoadComplete:
					//Debug.Log($"Client loaded sync scene: {sceneEvent.SceneName}");
					m_SynchedScenes.Add(new SceneReference(sceneEvent.Scene));
					break;
				case SceneEventType.UnloadComplete:
					//Debug.Log($"Client unloaded sync scene: {sceneEvent.SceneName}");
					m_SynchedScenes.Remove(new SceneReference(sceneEvent.Scene));
					break;
			}
		}

		private sealed class ProcessingScene
		{
			public SceneReference SceneRef;
			public SceneOperation Operation;
		}

		private enum SceneOperation
		{
			Load,
			Unload,
		}
	}
}
