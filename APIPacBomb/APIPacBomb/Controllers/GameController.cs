using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            Model.Map.Grid grid = new Model.Map.Grid(mapSettings.Width, mapSettings.Height, mapSettings.SquareFactor, mapSettings.CaptureFlagCount, mapSettings.BombsAtStart);
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
                    string msg = Encoding.UTF8.GetString(buffer).Trim('\0');                    

                    try
                    {
                        JObject json = JObject.Parse(msg);
                        
                        // Gem collected
                        if ((string)json["class"] == "GemCollected")
                        {
                            bool found = false;
                            string itemId = (string)json["objectValue"]["itemId"];

                            for (int i = 0; i < pair.Map.Columns.Count; i++)
                            {
                                List<Model.Map.Tile> tilesWithItem = pair.Map.Columns[i].FindAll(r => r.HasItem);

                                if (tilesWithItem.Count == 0)
                                {
                                    continue;
                                }

                                for (int k = 0; k < tilesWithItem.Count; k++)
                                {
                                    Model.Map.Tile tile = tilesWithItem[k];

                                    if (tile.Item.Id.ToString().ToLower() == itemId.ToLower())
                                    {
                                        pair.Map.Columns[tile.Item.Column][tile.Item.Row].Item = null;
                                        found = true;
                                        break;
                                    }
                                }

                                if (found)
                                {
                                    break;
                                }
                            }                            
                        }

                        _SessionService.UpdatePlayingPair(pair);
                    }
                    catch (JsonReaderException e)
                    {
                        _Logger.LogWarning(string.Format("Nachricht von {0} der Spielerpaarung {1} ist kein JSON :{2}", userId, pair.Id.ToString(), msg));
                    }
                    
                    await partnerWebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);                    
                }

                buffer = new byte[1024 * 4];
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
                pair.Map.PlayingPairId = pair.Id;
                pair.Map.GenerateMap();

                _SessionService.UpdatePlayingPair(pair);

                _Logger.LogInformation(string.Format("Spielkarte für Spielerpaarung {0} generiert.", pair.Id.ToString()));

                // Map an Spieler senden
                string mapJson = new Classes.Responses.WebSocketObjectResponse()
                {
                    Class = pair.Map.GetType().Name,
                    ObjectValue = pair.Map
                }.ToJsonString();                    

                // Warten, dass alle Partner ihren Handler initialisiert haben
                Thread.Sleep(500);

                await _SessionService.SendMessageToAllPlayers(pair.Id.ToString(), mapJson);

                _Logger.LogInformation(string.Format("Spielkarte an alle Spieler der Paarung {0} gesendet.", pair.Id.ToString()));

                // Auf Verarbeitung der letzten Information warten
                Thread.Sleep(500);

                _Logger.LogInformation(string.Format("Spielermodels fuer Spielerpaarung {0} wird erzeugt.", pair.Id.ToString()));

                int columnIndex = pair.Map.RowCount / 2;

                if (pair.Map.Columns[0][columnIndex].Type == Model.Map.Type.WALL)
                {
                    columnIndex++;
                }

                int rowIndex = pair.Map.ColumnCount / 2;

                if (pair.Map.Columns[rowIndex][1].Type == Model.Map.Type.WALL)
                {
                    rowIndex++;
                }

                pair.RequestedUser.Bomberman = new Model.Map.Bomberman()
                {
                    UserId = pair.RequestedUser.Id,
                    Username = pair.RequestedUser.Username,
                    X = pair.Map.Columns[0][columnIndex].DownLeft.X,
                    Y = pair.Map.Columns[0][columnIndex].DownLeft.Y,
                    Width = pair.Map.SquareFactor,
                    Size = pair.Map.SquareFactor,
                    OwnedFlag = new Model.Map.Flag()
                    {
                        X = pair.Map.Columns[rowIndex][1].DownLeft.X,
                        Y = pair.Map.Columns[rowIndex][1].DownLeft.Y,
                        Size = pair.Map.SquareFactor,
                        Color = Model.Map.PlayerColor.BLUE
                    }
                };

                pair.RequestingUser.Bomberman = new Model.Map.Bomberman()
                {
                    UserId = pair.RequestingUser.Id,
                    Username = pair.RequestingUser.Username,
                    X = pair.Map.Columns[pair.Map.ColumnCount - 1][columnIndex].DownLeft.X,
                    Y = pair.Map.Columns[pair.Map.ColumnCount - 1][columnIndex].DownLeft.Y,
                    Width = pair.Map.SquareFactor,
                    Size = pair.Map.SquareFactor,
                    OwnedFlag = new Model.Map.Flag()
                    {
                        X = pair.Map.Columns[rowIndex][pair.Map.RowCount - 3].DownLeft.X,
                        Y = pair.Map.Columns[rowIndex][pair.Map.RowCount - 3].DownLeft.Y,
                        Size = pair.Map.SquareFactor,
                        Color = Model.Map.PlayerColor.RED
                    }
                };

                _SessionService.UpdatePlayingPair(pair);

                string bomberManJson = new Classes.Responses.WebSocketObjectResponse()
                {
                    Class = pair.RequestingUser.Bomberman.GetType().Name,
                    ObjectValue = pair.RequestingUser.Bomberman
                }.ToJsonString();

                // An Spieler senden
                await _SessionService.SendMessageToAllPlayers(pair.Id.ToString(), bomberManJson);

                bomberManJson = new Classes.Responses.WebSocketObjectResponse()
                {
                    Class = pair.RequestedUser.Bomberman.GetType().Name,
                    ObjectValue = pair.RequestedUser.Bomberman
                }.ToJsonString();                

                await _SessionService.SendMessageToAllPlayers(pair.Id.ToString(), bomberManJson);


                _Logger.LogInformation(string.Format("Spielermodels fuer Spielerpaarung {0} an alle Teilnehmer gesendet.", pair.Id.ToString()));

                // Eventlistener für Itemgenerierung zuweisen und Generierungporzess starten
                pair.Map.ItemGenerated += _OnRandomGemGenerated;
                pair.Map.StartRandomGeneration(1500);

                _SessionService.UpdatePlayingPair(pair);
            }
        }

        /// <summary>
        ///   Wird ausgelöst, wenn ein zufällig generiertes Item auf dem Grid abgelegt wird
        /// </summary>
        /// <param name="sender">Objekt, das das Event ausgelöst hat</param>
        /// <param name="args">Eventargumente</param>
        private void _OnRandomGemGenerated(object sender, Classes.ItemGeneratedEventArgs args)
        {
            _Logger.LogInformation(string.Format("Neues Item für Spielerpaar {0} generiert.", args.Grid.PlayingPairId.ToString()));

            string gemJson = new Classes.Responses.WebSocketObjectResponse()
            {
                Class = args.NewGem.GetType().Name,
                ObjectValue = args.NewGem
            }.ToJsonString();

            _SessionService.SendMessageToAllPlayers(args.Grid.PlayingPairId.ToString(), gemJson);

            _Logger.LogInformation(string.Format("Item an Spielerpaar {0} gesendet.", args.Grid.PlayingPairId.ToString()));
        }
    }
}
