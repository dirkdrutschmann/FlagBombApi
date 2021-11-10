using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace APIPacBomb.Middleware
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _Next;
        
        private WebSocketHandler _WebSocketHandler { get; set; }

        public WebSocketManagerMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
        {
            _Next = next;
            _WebSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _WebSocketHandler.OnConnected(socket);

            await _Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _WebSocketHandler.ReceiveAsync(socket, result, buffer);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _WebSocketHandler.OnDisconnected(socket);                    
                }
            });
        }

       private async Task _Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
       {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }            
       }
    }
}
