using Enums;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace LGNContext
{
    public class LGNContextInfo
    {
	    public static EAppContext AppContext { get; set; } = EAppContext.Default;
	    private static string kInMeeting = "inMeeting";
        private static string kHost = "host";

        public static bool InMeeting
        {
#if UNITY_WEBGL
            get => CI_getRunningContext() == kInMeeting;
#else
            get => false;
#endif
        }

        public static string RunningContext
        {
#if UNITY_WEBGL
            get => CI_getRunningContext();
#else
            get => "";
#endif            
        }
        
        public static string MeetingID
        {
#if UNITY_WEBGL
            get => CI_getMeetingID();
#else
            get => "";
#endif
        }
        
        public static string MeetingUUID
        {
#if UNITY_WEBGL
	   get => CI_getMeetingUUID();
#else
           get => "";
#endif	        
        }

        public static string GeneratedUniqueUUID
        {
            get => MeetingID + MeetingUUID;
        }
        
        public static string Role
        {
#if UNITY_WEBGL
			get => CI_getRole();
#else
            get => "";
#endif	        
        }

        public static bool IsHost
        {
            get => Role == kHost;
        }
        
        public static string ScreenName 
        {
#if UNITY_WEBGL
           get => CI_getScreenName();
#else
           get => "";
#endif               
        }
        
        public static string SessionId 
        {
#if UNITY_WEBGL
           get => CI_getSessionId();
#else
	        get => "";
#endif               
        }
        
#if UNITY_WEBGL
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern string CI_getRunningContext();
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern string CI_getMeetingID();
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern string CI_getMeetingUUID();
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern string CI_getRole();
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern string CI_getScreenName();

	    [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern string CI_getSessionId();
#endif
    }
}
