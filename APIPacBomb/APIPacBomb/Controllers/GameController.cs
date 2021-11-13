using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIPacBomb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private Interfaces.ISessionService _SessionService;

        /// <summary>
        ///   Erstellt eine Instanz der Game-Controller-Klasse
        /// </summary>
        /// <param name="sessionService">SessionService-Instanz</param>
        public GameController(Interfaces.ISessionService sessionService)
        {
            _SessionService = sessionService;
        }

        /// <summary>
        ///   Generiert die Map
        /// </summary>
        /// <param name="mapSettings">Map-Settings</param>
        /// <returns>Map-Grid</returns>
        [Authorize]
        [HttpGet("map")]
        public IActionResult Get([FromBody] Classes.Requests.MapRequest mapSettings)
        {
            Model.Map.Grid grid = new Model.Map.Grid(mapSettings.Width, mapSettings.Height, mapSettings.SquareFactor);
            grid.GenerateMap();

            return Ok(grid);
        }

        /// <summary>
        ///   Initialisiert den WebSocket
        /// </summary>
        /// <param name="playingPairId">Spielerpaar-Id</param>
        /// <param name="userId">Eigene User-Id</param>
        /// <returns></returns>
        [HttpGet("ws/{playingPairId}/{userId}")]
        public async Task Get(string playingPairId, int userId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (WebSocket socket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    _SessionService.SetWebSocket(socket, playingPairId, userId, HttpContext);

                    await _Echo(HttpContext, socket, playingPairId, userId);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task _Echo(HttpContext context, WebSocket webSocket, string playingPairId, int userId)
        {
            // Partnersocket ermitteln
            Classes.UserPlayingPair pair = _SessionService.GetUserPlayingPair(playingPairId);
            WebSocket partnerWebSocket = _SessionService.GetPartnerWebSocket(playingPairId, userId);
            Model.User user = null;

            if (pair.RequestedUser.Id == userId)
            {
                user = pair.RequestedUser;
            }
            else
            {
                user = pair.RequestingUser;
            }


            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                if (partnerWebSocket != null)
                {
                    string message = "Nachricht: " + System.Text.Encoding.UTF8.GetString(buffer);

                    await partnerWebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    await webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message, 0, message.Length)), WebSocketMessageType.Text, true, CancellationToken.None);
                }                

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
