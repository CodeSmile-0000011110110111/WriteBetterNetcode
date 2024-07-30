// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeSmileEditor.WebRequest
{
	public partial class EditorWebRequest
	{
		private readonly String m_BaseUri;

		protected virtual IDictionary<String, String> RequestHeader => null;

		private static Action<UnityWebRequest> ResponseText(Action<String> onResponseText) =>
			www => onResponseText?.Invoke(www.downloadHandler.text);

		public EditorWebRequest(String baseUri) => m_BaseUri = baseUri;

		private String CreateUri(String endpoint) => $"{m_BaseUri}{endpoint}";

		private void StartCoroutine(IEnumerator coroutine) => EditorCoroutineUtility.StartCoroutine(coroutine, this);

		private UnityWebRequest CreateGetRequest(String endpoint) => UnityWebRequest.Get(CreateUri(endpoint));

		private UnityWebRequest CreatePutRequest(String endpoint, String data) =>
			UnityWebRequest.Put(CreateUri(endpoint), data);

		private UnityWebRequest CreatePostRequest(String endpoint, String contentType, String data) =>
			UnityWebRequest.Post(CreateUri(endpoint), data, contentType);

		public void Get(String endpoint, Action<String> onResponse) =>
			StartCoroutine(SendRequest(CreateGetRequest(endpoint), ResponseText(onResponse)));

		public void Get(String endpoint, Action<UnityWebRequest> onResponse) =>
			StartCoroutine(SendRequest(CreateGetRequest(endpoint), onResponse));

		public void Put(String endpoint, String data, Action<String> onResponse) =>
			StartCoroutine(SendRequest(CreatePutRequest(endpoint, data), ResponseText(onResponse)));

		public void Put(String endpoint, String data, Action<UnityWebRequest> onResponse) =>
			StartCoroutine(SendRequest(CreatePutRequest(endpoint, data), onResponse));

		public void Post(String endpoint, String contentType, String data, Action<String> onResponse) =>
			StartCoroutine(SendRequest(CreatePostRequest(endpoint, contentType, data), ResponseText(onResponse)));

		public void Post(String endpoint, String contentType, String data, Action<UnityWebRequest> onResponse) =>
			StartCoroutine(SendRequest(CreatePostRequest(endpoint, contentType, data), onResponse));

		private IEnumerator SendRequest(UnityWebRequest www, Action<UnityWebRequest> onResponse)
		{
			using (www)
			{
				var header = RequestHeader;
				if (header != null && header.Count > 0)
				{
					SetHeader(www, header);
					LogHeader(header);
				}

				LogRequest(www);

				yield return www.SendWebRequest();

				LogResponse(www);
				LogErrorResponse(www);

				if (www.result == UnityWebRequest.Result.Success)
					onResponse?.Invoke(www);
			}
		}

		private void SetHeader(UnityWebRequest www, IDictionary<String, String> header)
		{
			foreach (var kvp in header)
				www.SetRequestHeader(kvp.Key, kvp.Value);
		}

		private void LogErrorResponse(UnityWebRequest www)
		{
			if (www.result == UnityWebRequest.Result.InProgress)
				throw new ArgumentException("unexpected 'InProgress' result after yield of web request");

			if (www.result == UnityWebRequest.Result.ConnectionError ||
			    www.result == UnityWebRequest.Result.ProtocolError ||
			    www.result == UnityWebRequest.Result.DataProcessingError)
				LogError(www);
		}

		protected static class ContentType
		{
			public static String Json => "application/json";
		}
	}
}
