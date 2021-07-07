#if UNITY_WEBGL
using System;
using System.Runtime.InteropServices;
using AOT;
using Constants;
using UnityEngine.Events;

namespace WebGLUtil
{
    public class System
    {
        public delegate void PasteFromClipboardDelegate(String text);
        private delegate void _PasteFromClipboardComplete();

        public static PasteFromClipboardDelegate OnPasteFromClipboard;

        private static IntPtr _clipboardBuf;

        public static bool IsZoom
        {
            get
            {
                string zoom = "";
                GetURLSearchParam(URLParams.ZOOM, ref zoom);
                return zoom.ToLower() == "true";
            }
        }
        
        public static bool IsLGN
        {
            get
            {
                string lgn = "";
                GetURLSearchParam(URLParams.LGN, ref lgn);
                return lgn.ToLower() == "true";
            }
        }
        
        public static void PasteTextFromClipboard()
        {
            _clipboardBuf = Marshal.AllocHGlobal(512);
            pasteTextFromClipboard(__OnPasteFromClipboard, _clipboardBuf);
        }

        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern void pasteTextFromClipboard(_PasteFromClipboardComplete pasteFromClipboardDelegate, IntPtr clipboardBuf);
        
        [MonoPInvokeCallback(typeof(_PasteFromClipboardComplete))]
        private static void __OnPasteFromClipboard()
        {
            string value = Marshal.PtrToStringAnsi(_clipboardBuf);
            Marshal.FreeHGlobal(_clipboardBuf);
            OnPasteFromClipboard?.Invoke(value);
        }

        public static void CopyTextToClipboard(string text)
        {
            copyTextToClipboard(text);
        }
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern void copyTextToClipboard(string text);

        public static void GetURLSearchParam(string name, ref string value)
        {
            IntPtr val = Marshal.AllocHGlobal(512);
            getURLSearchParam(name, val);
            value = Marshal.PtrToStringAnsi(val);
            Marshal.FreeHGlobal(val);
        }
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern void getURLSearchParam(string name, IntPtr val);

        public static void OpenURL(string url, string target)
        {
            openURL(url, target);
        }
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        private static extern void openURL(string url, string target);
        
        public static void GetURL(ref string url)
        {
            IntPtr inURL = Marshal.AllocHGlobal(512);
            getURL(inURL);
            url = Marshal.PtrToStringAnsi(inURL);
            Marshal.FreeHGlobal(inURL);
        }
        
        [DllImport("__Internal", CharSet = CharSet.Ansi)]
        
        private static extern void getURL(IntPtr url);
    }
}
#endif