using Enums;
using KitsuneCommon.GameServer;
using KitsuneCore.Context;

namespace LGNContext
{
	public static class Capabilities
	{
		public static void Init()
		{
			EAppContext appContext = EAppContext.Default;
#if UNITY_WEBGL
			if (WebGLUtil.System.IsZoom)
			{
				appContext = EAppContext.Zoom;
				Debug.Log("Zoom MeetingUUID: " + LGNContextInfo.MeetingUUID);
				Debug.Log("Zoom User Role: " + LGNContextInfo.Role);
			}
			else if (WebGLUtil.System.IsLGN)
			{
				appContext = EAppContext.LGN;
				Debug.Log("LGN MeetingUUID: " + LGNContextInfo.MeetingUUID);
				Debug.Log("LGN User Role: " + LGNContextInfo.Role);
				Debug.Log("LGN SessionId: " + LGNContextInfo.SessionId);
			}
#endif
			LGNContextInfo.AppContext = appContext;

			switch (appContext)
			{
				case EAppContext.Zoom:
					KitsuneGameServer.Environment = EEnvironment.Mk("zoom_poker");
					break;
			}
			
			switch (appContext)
			{
				case EAppContext.Zoom:
					_internalVideoEnabled = false;
					_externalMeetingContext = true;
					_inGameAdvertisingEnabled = false;
					_loginWithSessionCookie = true;
					_gameSessionCodesEnabled = false;
					_useExternalPlayerName = true;
					break;
				case EAppContext.LGN:
					_internalVideoEnabled = false;
					_externalMeetingContext = true;
					_loginWithSessionCookie = true;
					_gameSessionCodesEnabled = false;
					break;
			}
		}

		private static bool _internalVideoEnabled = true;
		public static bool InternalVideoEnabled
		{
			get => _internalVideoEnabled;
			private set => _internalVideoEnabled = value;
		}
		
		private static bool _externalMeetingContext = false;
		public static bool ExternalMeetingContext
		{
			get => _externalMeetingContext;
			private set => _externalMeetingContext = value;
		}

		private static bool _inGameAdvertisingEnabled = true;
		public static bool InGameAdvertisingEnabled
		{
			get => _inGameAdvertisingEnabled;
			private set => _inGameAdvertisingEnabled = value;
		}

		private static bool _gameSessionCodesEnabled = true;
		public static bool GameSessionCodesEnabled
		{
			get => _gameSessionCodesEnabled;
			private set => _gameSessionCodesEnabled = value;
		}
		
		private static bool _loginWithSessionCookie = false;
		public static bool LoginWithSessionCookie
		{
			get => _loginWithSessionCookie;
			private set => _loginWithSessionCookie = value;
		}

		private static bool _useExternalPlayerName;
		public static bool UseExternalPlayerName
		{
			get => _useExternalPlayerName;
			private set => _useExternalPlayerName = value;
		}
	}
}