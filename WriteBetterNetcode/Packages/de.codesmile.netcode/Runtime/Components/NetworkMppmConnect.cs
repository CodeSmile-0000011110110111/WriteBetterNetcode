// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using CodeSmile.SceneTools;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
#endif

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Multiplayer testing in playmode! Handles start and shutdown of 'Multiplayer Playmode' (MPPM) editor & virtual
	///     players. Player tags determine what role each player is supposed to launch in.
	/// </summary>
	/// <remarks>
	///     Recognized tags (case sensitive!) are editable, defaults: "Server", "Host", "Client".
	///     Only one active player should have either the Server or Host tag.
	///     If a player has no tags that player will not start a network session.
	/// </remarks>
	[DisallowMultipleComponent]
	public class NetworkMppmConnect : MonoBehaviour
	{
		private const String JoinCodeFile = "RelayJoinCode.txt";
		private const String TryRelayFile = "TryRelayInEditor.txt";
		private const string DefaultServerTag = "Server";
		private const string DefaultHostTag = "Host";
		private const string DefaultClientTag = "Client";

		// copied from NetworkManagerEditor
		private static readonly String k_UseEasyRelayIntegrationKey =
			"NetworkManagerUI_UseRelay_" + Application.dataPath.GetHashCode();

		private static Boolean s_AutoUnpauseOnEnterPlaymode;

		[Tooltip("MPPM errors on shutdown can leave the main editor state paused with the pause button not visually pressed. " +
		         "Starting playmode with virtual players in this case will have virtual players seem frozen. " +
		         "By setting this true, entering playmode will unpause the game for all virtual players. " +
		         "Uncheck if you intentionally want to start paused for debugging purposes.")]
		[SerializeField] private Boolean m_AutoUnpauseOnEnterPlaymode = true;

		[SerializeField] private String m_ServerTag = DefaultServerTag;
		[SerializeField] private String m_HostTag = DefaultHostTag;
		[SerializeField] private String m_ClientTag = DefaultClientTag;

		private Boolean m_WillStartNetworking;

#if UNITY_EDITOR
		private void Awake()
		{
			s_AutoUnpauseOnEnterPlaymode = m_AutoUnpauseOnEnterPlaymode;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			CheckInvalidTagAssignment();
		}

		private void Start() => TryStartMultiplayerPlaymodeEndOfFrame();

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredPlayMode)
			{
				if (s_AutoUnpauseOnEnterPlaymode && EditorApplication.isPaused)
				{
					EditorApplication.isPaused = false;
					Debug.LogWarning($"{nameof(NetworkMppmConnect)}: EditorApplication.isPaused was true entering " +
					                 "playmode => auto unpaused to let virtual players start up");
				}
			}
		}

		private async void TryStartMultiplayerPlaymodeEndOfFrame()
		{
			if (ShouldStartServer || ShouldStartHost || ShouldStartClient)
				SceneAutoLoader.DestroyAll(); // will auto-load scene when networking begins

			if (ShouldStartServer || ShouldStartHost)
			{
				DeleteRelayJoinCodeFile();
				DeleteEditorRelayEnabledFile();
			}

			await TryStartMultiplayerPlaymode();
			enabled = false;
		}

		public Boolean ShouldStartServer => CurrentPlayer.ReadOnlyTags().Contains(m_ServerTag);
		public Boolean ShouldStartHost => CurrentPlayer.ReadOnlyTags().Contains(m_HostTag);
		public Boolean ShouldStartClient => CurrentPlayer.ReadOnlyTags().Contains(m_ClientTag);

		private async Task TryStartMultiplayerPlaymode()
		{
			if (ShouldStartServer)
				await StartAsServer();
			else if (ShouldStartHost)
				await StartAsHost();
			else if (ShouldStartClient)
				await StartAsClient();
		}

		private async Task StartAsServer()
		{
			var useRelay = IsEditorRelayEnabled();
			SetEditorRelayEnabled(useRelay);
			NetworkLog.LogInfo("Multiplayer Playmode => Start SERVER");

			await NetcodeUtility.StartServer();

			if (NetworkManager.Singleton.IsServer == false)
			{
				throw new Exception(
					"==> MPPM: failed to start server! Check if multiple players have the Host/Server tag assigned.");
			}

			if (useRelay)
				WriteRelayJoinCodeFile();

			NetworkLog.LogInfo("Multiplayer Playmode => SERVER did start ...");
		}

		private async Task StartAsHost()
		{
			var useRelay = IsEditorRelayEnabled();
			SetEditorRelayEnabled(useRelay);
			NetworkLog.LogInfo("Multiplayer Playmode => Start HOST");

			await NetcodeUtility.StartHost();

			if (NetworkManager.Singleton.IsHost == false)
			{
				throw new Exception(
					"==> MPPM: failed to start host! Check if multiple players have the Host/Server tag assigned.");
			}

			if (useRelay)
				WriteRelayJoinCodeFile();

			NetworkLog.LogInfo("Multiplayer Playmode => HOST did start ...");
		}

		private async Task StartAsClient()
		{
			await Task.Delay(250); // ensure a virtual client never starts before the host

			NetcodeUtility.UseRelayService = await WaitForEditorRelayEnabledFile();
			if (NetcodeUtility.UseRelayService)
			{
				NetworkLog.LogInfo("Multiplayer Playmode => CLIENT waiting for relay " +
				                   $"join code in: {RelayJoinCodeFilePath}");
				var code = await WaitForRelayJoinCodeFile();
				NetcodeUtility.RelayJoinCode = code;

				NetworkLog.LogInfo($"Multiplayer Playmode => CLIENT got relay join code: {code}");
			}

			NetworkLog.LogInfo("Multiplayer Playmode => Start CLIENT");
			await NetcodeUtility.StartClient();

			if (NetworkManager.Singleton.IsClient == false)
				throw new Exception("==> MPPM: failed to start client!");

			NetworkLog.LogInfo("Multiplayer Playmode => CLIENT connecting ...");
		}

		private void CheckInvalidTagAssignment()
		{
			var tags = CurrentPlayer.ReadOnlyTags();
			var tagCount = tags.Contains(m_ServerTag) ? 1 : 0;
			tagCount += tags.Contains(m_HostTag) ? 1 : 0;
			tagCount += tags.Contains(m_ClientTag) ? 1 : 0;

			if (tagCount > 1)
				throw new ArgumentException("==> MPPM: multiple conflicting tags assigned to player");
		}

		private void SetEditorRelayEnabled(Boolean enabled)
		{
			NetcodeUtility.UseRelayService = enabled;
			WriteEditorRelayEnabledFile(enabled);
			if (enabled)
				NetworkLog.LogInfo("Multiplayer Playmode => Using Relay service ...");
		}

		private static Boolean IsEditorRelayEnabled() => EditorPrefs.GetBool(k_UseEasyRelayIntegrationKey, false);

		private static String EditorRelayEnabledFilePath => $"{Application.persistentDataPath}/{TryRelayFile}";

		private async Task<Boolean> WaitForEditorRelayEnabledFile()
		{
			do
			{
				await Task.Delay(200);
			} while (EditorRelayEnabledFileExists() == false);

			return ReadEditorRelayEnabledFile();
		}

		private void WriteEditorRelayEnabledFile(Boolean useRelay)
		{
			try
			{
				var path = EditorRelayEnabledFilePath;
				File.WriteAllText(path, useRelay.ToString());
				// Debug.Log($"Wrote try-relay-in-editor {useRelay} to file: {path}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private Boolean ReadEditorRelayEnabledFile()
		{
			try
			{
				var path = EditorRelayEnabledFilePath;
				var useRelay = File.ReadAllText(path);
				// Debug.Log($"Read try-relay-in-editor {useRelay} from file: {path}");
				return useRelay.Equals(true.ToString());
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			return false;
		}

		private void DeleteEditorRelayEnabledFile()
		{
			try
			{
				var path = EditorRelayEnabledFilePath;
				if (EditorRelayEnabledFileExists())
				{
					File.Delete(path);
					// Debug.Log($"Deleted try-relay-in-editor exchange file: {path}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private static Boolean EditorRelayEnabledFileExists() => File.Exists(EditorRelayEnabledFilePath);

		private static String RelayJoinCodeFilePath => $"{Application.persistentDataPath}/{JoinCodeFile}";

		private async Task<String> WaitForRelayJoinCodeFile()
		{
			do
			{
				await Task.Delay(200);
			} while (RelayJoinCodeFileExists() == false);

			return ReadRelayJoinCodeFile();
		}

		private static void WriteRelayJoinCodeFile()
		{
			try
			{
				var path = RelayJoinCodeFilePath;
				File.WriteAllText(path, NetcodeUtility.RelayJoinCode);
				// Debug.Log($"Wrote join code {Network.RelayJoinCode} to file: {path}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private static String ReadRelayJoinCodeFile()
		{
			String joinCode = null;
			try
			{
				var path = RelayJoinCodeFilePath;
				joinCode = File.ReadAllText(path);
				// Debug.Log($"Read join code {joinCode} from file: {path}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			return joinCode;
		}

		private static void DeleteRelayJoinCodeFile()
		{
			try
			{
				var path = RelayJoinCodeFilePath;
				if (File.Exists(path))
				{
					File.Delete(path);
					// Debug.Log($"Deleted join code exchange file: {path}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private static Boolean RelayJoinCodeFileExists() => File.Exists(RelayJoinCodeFilePath);
#endif
	}
}
