// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeSmileEditor.WebRequest
{
	public partial class EditorWebRequest
	{
		private static void LogHeader(IDictionary<String, String> header)
		{
			// if (Preferences.instance.Debug.LogWebRequests)
			{
				foreach (var kvp in header)
					Debug.Log($"Header: \"{kvp.Key}\" : \"{kvp.Value}\"");
			}
		}

		private static void LogRequest(UnityWebRequest www)
		{
			// if (Preferences.instance.Debug.LogWebRequests)
			{
				Debug.Log($"Request: {www.method} \"{www.uri}\"");
				if (www.uploadHandler != null)
				{
					Debug.Log($"Upload Data: \"{Encoding.UTF8.GetString(www.uploadHandler.data)}\" - " +
					          $"\"{www.uploadHandler.contentType}\"");
				}
			}
		}

		private static void LogResponse(UnityWebRequest www)
		{
			// if (Preferences.instance.Debug.LogWebRequests)
				Debug.Log($"Response: \"{www.downloadHandler.text}\"");
		}

		private static void LogError(UnityWebRequest www) => Debug.LogError(
			$"{www.result}: {www.error}, request: {www.method}, " +
			$"uri: {www.uri}, response: {www.downloadHandler.text}");
	}
}
