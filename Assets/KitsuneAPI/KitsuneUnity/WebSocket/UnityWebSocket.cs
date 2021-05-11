#if UNITY_WEBGL
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using AOT;
using KitsuneCommon.Debug;
using KitsuneCommon.Net.LowLevel;
using KitsuneCore.Net.LowLevel.SocketConnection;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity.WebSocket
{
	public class UnityWebSocket : IKitsuneWebSocket
	{
        public delegate void OnOpenCallback(int socketId);
        
        public delegate void OnMessageCallback(int socketId, System.IntPtr messagePointer, int messageSize);
        
        public delegate void OnCloseCallback(int socketId, int closeCode);

        public delegate void OnErrorCallback(int socketId, System.IntPtr errorPointer);

        /* KitsuneWebSocketLib JSLib*/
        [DllImport("__Internal")]
        public static extern int __Connect(int socketId, string host);

        [DllImport("__Internal")]
        public static extern int __Close(int socketId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int __Send(int socketId, byte[] buffer, int bufferLength);

        [DllImport("__Internal")]
        public static extern int __SocketState(int socketId);
        
        [DllImport("__Internal")]
        public static extern int __Create(int socketId);

        [DllImport("__Internal")]
        public static extern int __Dispose();

        [DllImport("__Internal")]
        public static extern void __OnOpen(int socketId, OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void __OnMessage(int socketId, OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void __OnClose(int socketId, OnCloseCallback callback);

        [DllImport("__Internal")]
        public static extern void __OnError(int socketId, OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void __Debug(int socketId);

        public int SocketId { get; }
        public string Host { get; private set; }

        public event BaseKitsuneWebSocket.OpenEventHandler OnOpen;
        public event BaseKitsuneWebSocket.MessageEventHandler OnMessage;
        public event BaseKitsuneWebSocket.ErrorEventHandler OnError;
        public event BaseKitsuneWebSocket.CloseEventHandler OnClose;

        private UnityWebSocket(int socketId)
        {
            SocketId = socketId;
        }

        public bool Connect(string host)
        {
            Host = host;
            
            // Output.Debug(this, "Connecting to host=" + Host);

            int ret = __Connect(SocketId, Host);
            
            if (ret < 0)
            {
                Output.Debug(this, "Connect Error=" + (ESocketError)ret);
                return false;
            }

            return true;
        }

        public void DebugMode()
        {
            __Debug(SocketId);
        }

        public EWebSocketState State
        {
            get
            {
                int state = __SocketState(SocketId);

                if (state < 0)
                {
                    Output.Debug(this, "Socket State Error=" + (ESocketError)state);
                }

                return (EWebSocketState) state;
            }
        }

        public void Close(EWebSocketCloseCode code, string reason = "")
        {
            int ret = __Close(SocketId, (int)code, reason);

            if (ret < 0)
            {
                Output.Error(this, "Close Error=" + (ESocketError)ret);
            }
        }

        public ESocketError Send(byte[] data)
        {
            int ret = __Send(SocketId, data, data.Length);

            // Output.Debug(this, "Send byte[] data with length=" + data.Length);

            ESocketError socketError = (ESocketError) ret;
            if (ret < 0)
            {
                Output.Debug(this, "Send Error=" + socketError);
            }

            return socketError;
        }
        
        private void InternalOnOpen()
        {
            // Output.Debug("UnityWebSocket", "InternalOnOpen");

            OnOpen?.Invoke();
        }

        private void InternalOnMessage(byte[] data)
        {
            // Output.Debug("UnityWebSocket", "InternalOnMessage - data length=" + data.Length);

            OnMessage?.Invoke(data);
        }

        private void InternalOnError(string errorMessage)
        {
            // Output.Debug("UnityWebSocket", "InternalOnError");

            OnError?.Invoke(errorMessage);
        }

        private void InternalOnClose(int closeCode)
        {
            // Output.Debug("UnityWebSocket", "InternalOnClose");

            OnClose?.Invoke((EWebSocketCloseCode)closeCode);
        }
        
        private static readonly ConcurrentDictionary<int, UnityWebSocket> _sockets = new ConcurrentDictionary<int, UnityWebSocket>();
        public static UnityWebSocket Create(ESocketId socketId)
        {
            // Output.Debug("UnityWebSocket", "Creating UnityWebSocket");

            int socketIntId = (int)socketId;
            
            if (_sockets.ContainsKey(socketIntId))
            {
                Debug.LogError("UnityWebSocket Create... SocketId=" + socketId + " already exists! Call Dispose before overwriting existing websockets");
                return null;
            }

            UnityWebSocket ws = new UnityWebSocket(socketIntId);
            
            __Create(ws.SocketId);
            __OnOpen(ws.SocketId, __OnOpenCallback);
            __OnMessage(ws.SocketId, __OnMessageCallback);
            __OnClose(ws.SocketId, __OnCloseCallback);
            __OnError(ws.SocketId, __OnErrorCallback);

            if (Debug.isDebugBuild)
            {
                ws.DebugMode();
            }
            
            _sockets[ws.SocketId] = ws;

            // Output.Debug("UnityWebSocket", "Creating UnityWebSocket= " + ws.SocketId);

            return ws;
        }

        public static void Dispose()
        {
            Debug.Log("Calling Dispose on all web sockets");
            
            foreach (var ws in _sockets.Values)
            {
                ws.OnClose = null;
                ws.OnMessage = null;
                ws.OnError = null;
                ws.OnError = null;
            }
           
            _sockets.Clear();

            int ret = __Dispose();
           
           if (ret < 0)
           {
               Output.Error("UnityWebSocket", "Dispose Error=" + (ESocketError)ret);
           }
        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void __OnOpenCallback(int socketId)
        {
            // Output.Debug("UnityWebSocket", "__OnOpenCallback= " + socketId);

            if (_sockets.TryGetValue(socketId, out UnityWebSocket ws))
            {
                // Output.Debug("UnityWebSocket", "_sockets.TryGetValue= " + ws);
                
                ws.InternalOnOpen();
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void __OnMessageCallback(int socketId, System.IntPtr messagePointer, int messageSize)
        {
            // Output.Debug("UnityWebSocket", "__OnMessageCallback= " + socketId);

            if (_sockets.TryGetValue(socketId, out UnityWebSocket ws))
            {
                // Output.Debug("UnityWebSocket", "_sockets.TryGetValue= " + ws);

                if (messagePointer == IntPtr.Zero)
                {
                    Output.Debug("UnityWebSocket", "messagePointer is null");
                }
                // Output.Debug("UnityWebSocket", "messageSize is=" + messageSize);

                byte[] message = new byte[messageSize];
                Marshal.Copy(messagePointer, message, 0, messageSize);

                ws.InternalOnMessage(message);
            }
        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void __OnErrorCallback(int socketId, System.IntPtr errorPointer)
        {
            // Output.Debug("UnityWebSocket", "__OnErrorCallback= " + socketId);

            if (_sockets.TryGetValue(socketId, out UnityWebSocket ws))
            {
                // Output.Debug("UnityWebSocket", "_sockets.TryGetValue= " + ws);

                string errorMsg = Marshal.PtrToStringAuto(errorPointer);
                ws.InternalOnError(errorMsg);
            }
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void __OnCloseCallback(int socketId, int closeCode)
        {
            // Output.Debug("UnityWebSocket", "__OnCloseCallback= " + socketId);

            if (_sockets.TryGetValue(socketId, out UnityWebSocket ws))
            {
                // Output.Debug("UnityWebSocket", "_sockets.TryGetValue= " + ws);

                ws.InternalOnClose(closeCode);
            }
        }

        public override string ToString()
        {
            return "SocketId=" + SocketId + " host=" + Host;
        }
    }
}
#endif