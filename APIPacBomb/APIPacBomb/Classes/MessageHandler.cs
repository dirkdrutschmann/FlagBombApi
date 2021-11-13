using APIPacBomb.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace APIPacBomb.Classes
{
    public class MessageHandler : WebSocketHandler
    {
        private Interfaces.ISessionService _sessionService;

        public MessageHandler(ConnectionManager webSocketConnectionManager, Interfaces.ISessionService sessionService) : base(webSocketConnectionManager)
        {
            _sessionService = sessionService;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);            

            var socketId = WebSocketConnectionManager.GetId(socket);     
                       

            await SendMessageToAllAsync($"{socketId} is now connected");
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            var message = $"{socketId} said: {Encoding.UTF8.GetString(buffer, 0, result.Count)}";

            await SendMessageToAllAsync(message);
        }
    }
}
