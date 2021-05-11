using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using KitsuneAPI.KitsuneUnity.Networking;
using KitsuneCommon.Debug;
using KitsuneCore;
using KitsuneCore.WebServer;
using UnityEngine;
using UnityEngine.Networking;

#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_XBOX_ONE)
using KitsuneCore.Net;
using KitsuneCommon.Net.LowLevel;
using KitsuneCore.Net.LowLevel.SocketConnection;
using KitsuneAPI.KitsuneUnity.WebSocket;
using KitsuneCore.Services.Authentication;
using KitsuneCore.Services.Places;
#endif

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Manager for the Kitsune PaaS.
	/// Attach this Script to a game object and it will auto-initialize the service using your GameSettings
	/// </summary>
	public class KitsuneManager : KitsuneSingleton<KitsuneManager>
	{
		public const string GAME_SETTINGS_FILE_NAME = "KitsuneGameSettings";
		public const string SERVER_LIST_FILE_NAME = "KitsuneServerList";

		/// <value>
		/// Serialized game settings
		/// </value>
		private static KitsuneGameSettings _gameSettings;
		public static KitsuneGameSettings GameSettings
		{
			get
			{
#if UNITY_EDITOR
				return Resources.Load<KitsuneGameSettings>(GAME_SETTINGS_FILE_NAME);
#endif				
				if (_gameSettings == null)
				{
					_gameSettings = Resources.Load<KitsuneGameSettings>(GAME_SETTINGS_FILE_NAME);
					if (_gameSettings == null)
					{
						Debug.LogWarning("_gameSettings is null! Trying with .asset extension");
						_gameSettings = Resources.Load<KitsuneGameSettings>(GAME_SETTINGS_FILE_NAME + ".asset");
					}
				}

				return _gameSettings;
			}
			set => _gameSettings = value;
		}
		
		protected override void Awake()
		{
			base.Awake();
			
			// use the Unity console for logging
			if (Debug.isDebugBuild)
			{
				Output.ExternalLogger = Log;
			}
			else
			{
				Output.SetLogLevel(Output.LogLevel.kNone);
			}
			
			// use Unity JSONUtility in lieu of Json.Net
#if !UNITY_EDITOR
// TODO - this doesn't parse everything correctly throwing an exception
// commenting out for now in case there was a reason I added back on 9/2020
			KitsuneFacade.SetJSONParseAction(ParseJSONObject);
#endif
			
#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_XBOX_ONE)
				KitsuneFacade.SetWebRequestAction(SendWebRequest);
				InitWebSockets();
			    Kitsune.Places.Subscribe<PlaceEvent.ON_SWITCHING_SERVERS>(OnSwitchingServers);
			    Kitsune.Authentication.Subscribe<AuthenticationEvent.ON_DISCONNECTED>(OnDisconnected);
				Kitsune.Authentication.Subscribe<AuthenticationEvent.ON_SERVER_DISCONNECTED>(OnServerDisconnect);
			    Kitsune.Authentication.Subscribe<AuthenticationEvent.ON_UNEXPECTED_DISCONNECT>(OnUnexpectedDisconnect);
#endif
			
			Kitsune.Init(GameSettings);
		}
		
#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_XBOX_ONE)
		private void OnServerDisconnect(string message, EServerDisconnectReason reason)
		{
			ReinitWebSockets();
		}

		private void OnSwitchingServers()
		{
			ReinitWebSockets();
		}

		private void OnDisconnected()
		{
			ReinitWebSockets();
		}

		private void OnUnexpectedDisconnect(KitsuneSocketException exception)
		{
			ReinitWebSockets();
		}

		private void ReinitWebSockets()
		{
			UnityWebSocket.Dispose();
			InitWebSockets();
		}

		public void InitWebSockets()
		{
			KitsuneFacade.UseWebSockets(UnityWebSocket.Create(ESocketId.Info), UnityWebSocket.Create(ESocketId.Town));
		}
#endif
		public void SendWebRequest(string servletUrl, Dictionary<string, string> queryParams, Dictionary<string, object> postValues, EUrlRequestMethod requestMethod, Action<string> responseCallback, Action<string> onError)
		{
			StartCoroutine(SendWebRequestCoroutine(servletUrl, queryParams, postValues, requestMethod, responseCallback, onError));
		}

		public IEnumerator SendWebRequestCoroutine(string servletUrl,
			Dictionary<string, string> queryParams, 
			Dictionary<string, object> postValues, 
			EUrlRequestMethod requestMethod,
			Action<string> responseCallback,
			Action<string> onError)
		{
			UnityWebRequest webRequest;
			switch (requestMethod)
			{
				case EUrlRequestMethod.GET:
					UriBuilder builder = new UriBuilder(servletUrl);
					string query = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;
					builder.Query = query;
				
					webRequest = UnityWebRequest.Get(builder.ToString());
					break;
				case EUrlRequestMethod.POST:
					string serializedData = JsonUtility.ToJson(postValues);
					webRequest = new UnityWebRequest(servletUrl, "POST");
					byte[] bodyRaw = Encoding.UTF8.GetBytes(serializedData);
					webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
					webRequest.downloadHandler = new DownloadHandlerBuffer();
					webRequest.SetRequestHeader("Content-Type", "application/json");
					break;
				default:
					WWWForm data = new WWWForm();
					foreach (KeyValuePair<string, string> param in queryParams)
					{
						data.AddField(param.Key, param.Value);
					}

					webRequest = UnityWebRequest.Post(servletUrl, data);
					break;
			}
			
			webRequest.certificateHandler = new AcceptAllLocalCerts();

			yield return webRequest.SendWebRequest();
			
			if(webRequest.isNetworkError || webRequest.isHttpError) 
			{
				Debug.Log(webRequest.error);
				onError(webRequest.error);
			}
			else 
			{
				string responseText = webRequest.downloadHandler.text;
				
				Debug.Log(servletUrl + " Web request complete!" + " Response Code: " + webRequest.responseCode + "\n" +
				          "response=" + responseText);

				responseCallback(responseText);
			}
		}

		public void ParseJSONObject(string servletUrl, object jsonObject, string jsonString, Action<object> parsedCallback)
		{
			try
			{
				Type responseType = jsonObject.GetType();
				object populatedObject = JsonUtility.FromJson(jsonString, responseType);

				parsedCallback?.Invoke(populatedObject);
			}
			catch (Exception e)
			{
				Output.Error(this, "Error parsing response=" + e.Message + " raw response=" + jsonString);
			}
		}
		
		public void LateUpdate()
		{
			UpdateInternal();
		}

		private void Log(string logMessage)
		{
			Debug.Log(logMessage);
		}

		private void UpdateInternal()
		{
#if DEVELOPMENT_BUILD			
			if (Debugging.SocketLogging)
			{
				KitsuneFacade.OutputQueuedDebugMessages();
			}
#endif
			bool doDispatch = true;
			bool socketExceptions = KitsuneFacade.HandleSocketExceptions();
			if (socketExceptions)
			{
				KitsuneFacade.ClearQueuedMessages();
				doDispatch = false;
			}
			
			if (!KitsuneFacade.Connected)
				return;
			
			while (doDispatch)
			{
				doDispatch = KitsuneFacade.ProcessQueuedMessages();
			}
		}
	}
}