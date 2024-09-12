// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components.Registry;
using CodeSmile.MultiPal.Scene;
using CodeSmile.Utility;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.MultiPal.GUI
{
	[DisallowMultipleComponent]
	public class DevSceneLoadTest : MenuBase
	{
		[SerializeField] private SceneReference m_Scene1;
		[SerializeField] private SceneReference m_Scene2;
		[SerializeField] private SceneReference m_Scene3;

		private Button LoadButton1 => m_Root.Q<Button>("Load1");
		private Button LoadButton2 => m_Root.Q<Button>("Load2");
		private Button LoadButton3 => m_Root.Q<Button>("Load3");
		private Button UnloadButton1 => m_Root.Q<Button>("Unload1");
		private Button UnloadButton2 => m_Root.Q<Button>("Unload2");
		private Button UnloadButton3 => m_Root.Q<Button>("Unload3");
		private Button LoadAllButton => m_Root.Q<Button>("LoadAll");
		private Button UnloadAllButton => m_Root.Q<Button>("UnloadAll");

		private ServerSceneLoader SceneLoader => ComponentsRegistry.Get<ServerSceneLoader>();

		private void OnEnable() => RegisterGuiEvents();
		private void OnDisable() => UnregisterGuiEvents();

		private void Start()
		{
			var isServer = NetworkManager.Singleton?.IsServer;
			if (isServer == false)
				Hide();
		}

		private void OnValidate()
		{
			m_Scene1.OnValidate();
			m_Scene2.OnValidate();
			m_Scene3.OnValidate();
		}

		private void RegisterGuiEvents()
		{
			LoadButton1.clicked += OnLoadButton1Clicked;
			LoadButton2.clicked += OnLoadButton2Clicked;
			LoadButton3.clicked += OnLoadButton3Clicked;
			UnloadButton1.clicked += OnUnloadButton1Clicked;
			UnloadButton2.clicked += OnUnloadButton2Clicked;
			UnloadButton3.clicked += OnUnloadButton3Clicked;
			LoadAllButton.clicked += OnLoadAllButtonClicked;
			UnloadAllButton.clicked += OnUnloadAllButtonClicked;
		}

		private void UnregisterGuiEvents()
		{
			LoadButton1.clicked -= OnLoadButton1Clicked;
			LoadButton2.clicked -= OnLoadButton2Clicked;
			LoadButton3.clicked -= OnLoadButton3Clicked;
			UnloadButton1.clicked -= OnUnloadButton1Clicked;
			UnloadButton2.clicked -= OnUnloadButton2Clicked;
			UnloadButton3.clicked -= OnUnloadButton3Clicked;
			LoadAllButton.clicked -= OnLoadAllButtonClicked;
			UnloadAllButton.clicked -= OnUnloadAllButtonClicked;
		}

		private void OnLoadButton1Clicked() => SceneLoader.LoadScenesAsync(new[] { m_Scene1 });
		private void OnLoadButton2Clicked() => SceneLoader.LoadScenesAsync(new[] { m_Scene2 });
		private void OnLoadButton3Clicked() => SceneLoader.LoadScenesAsync(new[] { m_Scene3 });
		private void OnLoadAllButtonClicked() => SceneLoader.LoadScenesAsync(new[] { m_Scene1, m_Scene2, m_Scene3 });
		private void OnUnloadButton1Clicked() => SceneLoader.UnloadScenesAsync(new[] { m_Scene1 });
		private void OnUnloadButton2Clicked() => SceneLoader.UnloadScenesAsync(new[] { m_Scene2 });
		private void OnUnloadButton3Clicked() => SceneLoader.UnloadScenesAsync(new[] { m_Scene3 });
		private void OnUnloadAllButtonClicked() => SceneLoader.UnloadScenesAsync(new[] { m_Scene1, m_Scene2, m_Scene3 });
	}
}
