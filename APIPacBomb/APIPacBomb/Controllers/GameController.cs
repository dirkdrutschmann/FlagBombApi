using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIPacBomb.Controllers
{
    /// <summary>
    ///   Controller für Spielrequests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private Interfaces.ISessionService _SessionService;

        private ILogger<GameController> _Logger;

        /// <summary>
        ///   Erstellt eine Instanz der Game-Controller-Klasse
        /// </summary>
        /// <param name="sessionService">SessionService-Instanz</param>
        public GameController(Interfaces.ISessionService sessionService, ILogger<GameController> logger)
        {
            _SessionService = sessionService;
            _SessionService.AllPartnersConnected += _OnAllPartnersConnected;

            _Logger = logger;
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
        ///   Prüft, ob der Spielpartner die WebSocket-Verbindung aufgebaut hat
        /// </summary>
        /// <param name="playingPairId">Spielerpaar-Id</param>
        /// <returns>StdResponse</returns>
        [Authorize]
        [HttpGet("isPartnerConnected/{playingPairId}")]
        public IActionResult IsPartnerConnected(string playingPairId)
        {
            Classes.UserPlayingPair pair = _SessionService.GetUserPlayingPair(playingPairId);
            Model.User user = _SessionService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));

            Classes.Responses.StdResponse response = new Classes.Responses.StdResponse();

            if 
            (
                (pair.RequestedUser.Id == user.Id && pair.RequestingUser.WebSocket != null && pair.RequestingUser.WebSocket.State != WebSocketState.Closed) ||
                (pair.RequestingUser.Id == user.Id && pair.RequestedUser.WebSocket != null && pair.RequestedUser.WebSocket.State != WebSocketState.Closed)
            )
            {
                response.Success = true;
                response.Message = "Spielpartner hat die Verbindung gestartet.";
            }
            else
            {
                response.Success = false;
                response.Message = "Spielpartner ist nicht verbunden.";
            }

            return Ok(response);
        }

        /// <summary>
        ///   Zeigt das Ende des Spieles an
        /// </summary>
        /// <param name="playingPairId">Spielerpaar-Id</param>
        /// <returns>StdResponse</returns>
        [Authorize]
        [HttpPost("setGameOver/{playingPairId}")]
        public IActionResult SetGameOver(string playingPairId)
        {
            Classes.Responses.StdResponse response = new Classes.Responses.StdResponse()
            {
                Success = true
            };

            try
            {
                Classes.UserPlayingPair pair = _SessionService.GetUserPlayingPair(playingPairId);

                if (pair.Status == Classes.UserPlayingPair.PlayingStatus.GAME_OVER)
                {
                    response.Message = "Spiel bereits vorbei.";
                    return Ok(response);
                }

                pair.Status = Classes.UserPlayingPair.PlayingStatus.GAME_OVER;
                _SessionService.UpdatePlayingPair(pair);

                response.Message = "Spiel vorbei";
            }
            catch (Classes.Exceptions.PlayingPairNotFoundException e)
            {
                response.Success = false;
                response.Message = e.Message;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, string.Format("Unerwarteter Fehler beim Setzen des GAME-OVER-Status für PlayingPairId {0}", playingPairId));
                response.Success = false;
                response.Message = e.Message;
            }

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
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

                    if (!socket.CloseStatus.HasValue)
                    {
                        await _Echo(HttpContext, socket, playingPairId, userId);
                    }                    
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
            Model.User partnerUser = null;

            if (pair.RequestedUser.Id == userId)
            {
                user = pair.RequestedUser;
                partnerUser = pair.RequestingUser;
            }
            else
            {
                user = pair.RequestingUser;
                partnerUser = pair.RequestedUser;
            }


            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                if (partnerWebSocket == null)
                {
                    partnerWebSocket = _SessionService.GetPartnerWebSocket(pair.Id.ToString(), user.Id);
                }

                if (partnerWebSocket != null && !partnerWebSocket.CloseStatus.HasValue)
                {
                    await partnerWebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);                    
                }                

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None); 
        }

        private async void _OnAllPartnersConnected(object sender, Classes.UserPlayingPair pair)
        {
            // Neue Karte generieren
            if (!pair.Map.IsGeneratedOnce)
            {
                pair.Status = Classes.UserPlayingPair.PlayingStatus.IN_GAME;
                pair.Map.GenerateMap();

                _SessionService.UpdatePlayingPair(pair);

                _Logger.LogInformation(string.Format("Spielkarte für Spielerpaarung {0} generiert.", pair.Id.ToString()));

                // Map an Spieler senden
                string mapJson = new Classes.Responses.WebSocketObjectResponse()
                {
                    Class = pair.Map.GetType().Name,
                    ObjectValue = pair.Map
                }.ToJsonString();                    

                // Eine Sekunde warten, dass alle Partner ihren Handler initialisiert haben
                Thread.Sleep(1000);

                await pair.RequestedUser.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(mapJson, 0, mapJson.Length)), WebSocketMessageType.Text, true, CancellationToken.None);
                await pair.RequestingUser.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(mapJson, 0, mapJson.Length)), WebSocketMessageType.Text, true, CancellationToken.None);

                _Logger.LogInformation(string.Format("Spielkarte an alle Spieler der Paarung {0} gesendet.", pair.Id.ToString()));
            }
        }
    }
}
