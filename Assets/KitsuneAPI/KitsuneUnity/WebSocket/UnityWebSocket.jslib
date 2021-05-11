var KitsuneWebSocketLib = {
    $webSockets: {},

    $kws_log: function (message) {
        // uncomment below for debug logging
        // console.log(message);
    },
    
    __Create: function (socketId)
    {
      var ws = {
          socket: null,
          url: null,
          socketId: socketId,
          onOpen: null,
          onMessage: null,
          onError: null,
          onClose: null,
          debugMode: false
      };
      
      webSockets[socketId] = ws;

      kws_log('[KitsuneWebSocket] WebSocketCreated=' + socketId);
      
      return 0;
    },
    
    __Connect: function(socketId, host)
    {
        kws_log('[KitsuneWebSocket] Connecting to=' + host);

        var jsHostString = Pointer_stringify(host);
        var ws = webSockets[socketId];
        ws.url = jsHostString;
        ws.socket = new WebSocket(jsHostString);
        ws.socket.binaryType = 'arraybuffer';
        ws.socket.onopen = function(e)
        {
            if (ws.debugMode)
            {
                kws_log('[KitsuneWebSocket] Connected=' + socketId);
                kws_log(e);
            }
            
            if (ws.onOpen)
            {
                kws_log('[KitsuneWebSocket] ws.onOpen');

                Runtime.dynCall('vi', ws.onOpen, [ socketId ]);
            }
        }

        ws.socket.onmessage = function(e)
        {
            if (ws.debugMode)
            {
                kws_log('[KitsuneWebSocket] WebSocket Message: ' + e.data);
                kws_log(e);
            }
            
            if (ws.onMessage) 
            {
                kws_log('[KitsuneWebSocket] ws.onMessage');

                if (e.data instanceof ArrayBuffer) 
                {
                    var dataBuffer = new Uint8Array(e.data);
                    var buffer = _malloc(dataBuffer.length);
                    HEAPU8.set(dataBuffer, buffer);

                    try 
                    {
                        Runtime.dynCall('viii', ws.onMessage, [socketId, buffer, dataBuffer.length]);
                    } 
                    finally 
                    {
                        _free(buffer);
                    }
                }
            }
        }

        ws.socket.onclose = function(e)
        {
            if (ws.debugMode)
            {
                kws_log('[KitsuneWebSocket] Closed.');
                kws_log(e);
            }

            if (ws.onClose)
            {
                kws_log('[KitsuneWebSocket] ws.onClose');
                
                Runtime.dynCall('vii', ws.onClose, [ socketId, e.code ]);
            }

            delete ws.socket;
        }

        ws.socket.onerror = function(e)
        {
            if (ws.debugMode)
            {
                kws_log('[KitsuneWebSocket] WebSocket Error: ' + e.data);
                kws_log(e);
            }
            
            if (ws.onError)
            {
                kws_log('[KitsuneWebSocket] ws.onError');

                var message = "WebSocket error.";
                var messageBytes = lengthBytesUTF8(message);
                var messageBuffer = _malloc(messageBytes + 1);
                stringToUTF8(message, messageBuffer, messageBytes);
                
                try 
                {
                    Runtime.dynCall('vii', ws.onError, [ socketId, messageBuffer ]);
                }
                finally 
                {
                    _free(messageBuffer);
                }
            }
        }
    },

    __Send: function(socketId, buffer, length)
    {
        var ws = webSockets[socketId];
        if (!ws) 
        {
            return -11;
        }

        if (!ws.socket)
        {
            return -17;
        }

        if (ws.socket.readyState !== 1)
        {
            return -14;
        }

        kws_log('[KitsuneWebSocket] Sending Message of length= ' + length);

        ws.socket.send(HEAPU8.buffer.slice(buffer, buffer + length));

        return 0;
    },
    
    __Close: function(socketId, code, reason)
    {
        kws_log('[KitsuneWebSocket] __Close Closing socketId=' + socketId);

        var ws = webSockets[socketId];
        if (!ws) 
        {
            return -11;
        }

        if (ws.socket === null)
        {
            return -17;
        }

        if (ws.socket.readyState === 2)
        {
            return -13;
        }

        if (ws.socket.readyState === 3)
        {
            return -14;
        }

        var jsReasonString = reason ? Pointer_stringify(reason) : undefined;

        try 
        {
            ws.socket.close(code, jsReasonString);
        } 
        catch(err) 
        {
            return -15;
        }

        return 0;
    },
    
    __Dispose: function()
    {
        for (socketId in webSockets) {
            if (webSockets.hasOwnProperty(socketId))
            {
                var ws = webSockets[socketId];

                if (!ws)
                {
                    delete webSockets[socketId];
                    continue;
                }

                if (ws.socket !== null && ws.socket !== undefined && ws.socket.readyState < 2)
                {
                    kws_log('[KitsuneWebSocket] __Dispose Disposed of socket socketId= ' + socketId + " but it is open or connecting... closing first");

                    ws.socket.close();
                }

                delete ws.socket;
                ws = null;

                delete webSockets[socketId];

                kws_log('[KitsuneWebSocket] __Dispose Disposed of socket socketId= ' + socketId);
            }
        }

        return 0;
    },
    
    __SocketState: function (socketId)
    {
        var ws = webSockets[socketId];
        if (!ws)
        {
            return -11;
        }
        
        if (ws.socket !== null && ws.socket !== undefined)
        {
            return ws.socket.readyState;
        }
        
        return 3; // closed
    },
    
    __OnOpen: function(socketId, callback)
    {
        var ws = webSockets[socketId];
        if (!ws)
        {
            return -11;
        }
        
        ws.onOpen = callback;
    },

    __OnMessage: function(socketId, callback)
    {
        kws_log('[KitsuneWebSocket] __OnMessage SocketId=' + socketId);

        var ws = webSockets[socketId];
        if (!ws)
        {
            return -11;
        }

        kws_log('[KitsuneWebSocket] __OnMessage Socket is Valid');


        ws.onMessage = callback;
    },

    __OnClose: function(socketId, callback)
    {
        kws_log('[KitsuneWebSocket] __OnClose SocketId=' + socketId);

        var ws = webSockets[socketId];
        if (!ws)
        {
            return -11;
        }

        kws_log('[KitsuneWebSocket] __OnClose Socket is Valid');

        ws.onClose = callback;
    },

    __OnError: function(socketId, callback)
    {
        kws_log('[KitsuneWebSocket] __OnError SocketId=' + socketId);

        var ws = webSockets[socketId];
        if (!ws)
        {
            return -11;
        }

        kws_log('[KitsuneWebSocket] __OnError Socket is Valid');

        ws.onError = callback;
    },
    
    __Debug: function(socketId)
    {
        kws_log('[KitsuneWebSocket] __Debug SocketId=' + socketId);

        var ws = webSockets[socketId];
        if (!ws) 
        {
            return -11;
        }

        kws_log('[KitsuneWebSocket] __Debug Socket is Valid');

        ws.debugMode = true;
    }
};

autoAddDeps(KitsuneWebSocketLib, '$kws_log');
autoAddDeps(KitsuneWebSocketLib, '$webSockets');
mergeInto(LibraryManager.library, KitsuneWebSocketLib);