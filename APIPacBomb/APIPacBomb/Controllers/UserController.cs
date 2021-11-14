using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIPacBomb.Controllers
{
    /// <summary>
    ///   Controllerklasse für Nutzerinteraktionen
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private Interfaces.IUserDatabaseService _userDatabaseService;
        private Interfaces.ISessionService _sessionService;
        private ILogger<UserController> _Logger;        

        /// <summary>
        ///  Erzeugt eine Instanz der Controllerklasse
        /// </summary>
        /// <param name="userDatabaseService">Nutzer-Datenbankservice</param>
        /// <param name="sessionService">Sessionservice</param>
        /// <param name="logger">Logginginstanz des Controllers</param>
        public UserController(Interfaces.IUserDatabaseService userDatabaseService, Interfaces.ISessionService sessionService, ILogger<UserController> logger)
        {
            _userDatabaseService = userDatabaseService;
            _sessionService = sessionService;
            _Logger = logger;            
        }

        /// <summary>
        ///   Liefert den Nutzer des JWT zurück
        /// </summary>
        /// <returns>Angemeldeter Nutzer</returns>
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_userDatabaseService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext)));
        }

        /// <summary>
        ///   Liefert alle angemeldeten Nutzer zurück
        /// </summary>
        /// <returns>Liste der angemeldeten Nutzer</returns>
        [Authorize]
        [HttpGet("All")]
        public IActionResult GetAll()
        {
            return Ok(_sessionService.GetLoggedInUsers());
        }

        /// <summary>
        ///   Sendet eine Spieleanfrage an den Nutzer mit der übergebenen Id
        /// </summary>
        /// <param name="id">UserId</param>
        /// <param name="mapConfig">Kartenkonfiguration</param>
        /// <returns>Standardresponse</returns>
        [Authorize]
        [HttpPost("PlayRequest/{id}")]
        public IActionResult PostPlayRequest(int id, [FromBody]Classes.Requests.MapRequest mapConfig)
        {
            Model.User user = _sessionService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));

            Classes.Responses.StdResponse response = new Classes.Responses.StdResponse();

            if (user.Id == id)
            {
                response.Success = false;
                response.Message = "Spieleanfragen können nur an andere Spieler gesendet werden.";

                return BadRequest(response);
            }


            try
            {
                _sessionService.SendPlayRequest(user, id, new Model.Map.Grid(mapConfig.Width, mapConfig.Height, mapConfig.SquareFactor), HttpContext);

                response.Success = true;
                response.Message = string.Format("Spieleanfrage erfolgreich an {0} gesendet.", _sessionService.GetUser(id).Username);
            }
            catch (Classes.Exceptions.NotLoggedInException e)
            {
                _Logger.LogError(e, string.Format("Fehler beim Senden der Spieleanfrage von UserId {0} an UserId {1}.", user.Id.ToString(), id.ToString()));

                response.Success = false;
                response.Message = e.Message;
            }
            catch (Classes.Exceptions.PlayRequestAlreadyExistsException e)
            {
                _Logger.LogError(e, string.Format("Fehler beim Senden der Spieleanfrage von UserId {0} an UserId {1}.", user.Id.ToString(), id.ToString()));

                response.Success = false;
                response.Message = e.Message;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, string.Format("Unerwarteter Fehler beim Senden der Spieleanfrage von UserId {0} an UserId {1}.", user.Id.ToString(), id.ToString()));

                response.Success = false;
                response.Message = "Fehler beim Senden der Spieleanfrage.";
            }

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        ///   Liefert alle Anfragen, die an einen selbst gestellt wurden
        /// </summary>        
        /// <returns>Liste der Spieleanfragen</returns>
        [Authorize]
        [HttpGet("PlayRequest/Incoming")]
        public IActionResult GetIncomingPlayRequest()
        {
            Model.User user = _sessionService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));

            return _GetPlayRequest(user, false);
        }

        /// <summary>
        ///   Liefert alle Anfragen, die man selbst gestellt hat
        /// </summary>        
        /// <returns>Liste der Spieleanfragen</returns>
        [Authorize]
        [HttpGet("PlayRequest/Outgoing")]
        public IActionResult GetOutgoingPlayRequest()
        {            
            Model.User user = _sessionService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));

            return _GetPlayRequest(user, true);
        }

        /// <summary>
        ///   Akzetiert eine Spieleanfrage des Nutzer mit der übergebenen UserId
        /// </summary>
        /// <param name="id">UserId des anfragenden Nutzers</param>
        /// <returns>StdResponse</returns>
        [Authorize]
        [HttpPost("AcceptPlayRequest/{id}")]
        public async Task<IActionResult> PostAcceptPlayRequest(int id)
        {
            Model.User requestedUser = _sessionService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));            

            try
            {
                _sessionService.AcceptPlayRequest(requestedUser, id, HttpContext);
                _Logger.LogInformation(string.Format("Spieleanfrage von UserId {0} durch UserId {1} akzeptiert.", id.ToString(), requestedUser.Id.ToString()));                

            }
            catch (Classes.Exceptions.PlayingPairNotFoundException e)
            {
                _Logger.LogError(e, string.Format("Spieleanfrage zum Akzptieren von UserId {0} an UserId {1} konnte nicht gefunden werden.", id.ToString(), requestedUser.Id.ToString()));

                return BadRequest(new Classes.Responses.StdResponse(false, e.Message));
            }
            catch (Classes.Exceptions.StateAlreadySetException e)
            {
                _Logger.LogError(e, string.Format("Spieleanfrage zum Akzptieren von UserId {0} an UserId {1} wurde bereits akzeptiert.", id.ToString(), requestedUser.Id.ToString()));
                return BadRequest(new Classes.Responses.StdResponse(false, e.Message));
            }
            catch (Exception e)
            {
                _Logger.LogError(e, string.Format("Unerwarteter Fehler beim Akzeptieren der Spieleanfrage von UserId {0} and UserId {1}.", id.ToString(), requestedUser.Id.ToString()));

                return BadRequest(new Classes.Responses.StdResponse(false, "Unerwarteter Fehler beim Akzeptieren der Spieleanfrage."));
            }

            
            return Ok(new Classes.Responses.StdResponse(true, "Spielanfrage akzeptiert."));
            
        }

        /// <summary>
        ///   Ablehnen einer Spieleanfrage des Nutzer mit der übergebenen UserId
        /// </summary>
        /// <param name="id">UserId des anfragenden Nutzers</param>
        /// <returns>StdResponse</returns>
        [Authorize]
        [HttpPost("RejectPlayRequest/{id}")]
        public IActionResult PostRejectPlayRequest(int id)
        {
            Model.User requestedUser = _sessionService.GetUser(Classes.Util.GetUsernameFromToken(HttpContext));

            try
            {
                _sessionService.RejectPlayRequest(requestedUser, id);
            }
            catch (Classes.Exceptions.PlayingPairNotFoundException e)
            {
                _Logger.LogError(e, string.Format("Spieleanfrage zum Ablehnen von UserId {0} an UserId {1} konnte nicht gefunden werden.", id.ToString(), requestedUser.Id.ToString()));

                return BadRequest(new Classes.Responses.StdResponse(false, e.Message));
            }
            catch (Classes.Exceptions.StateAlreadySetException e)
            {
                _Logger.LogError(e, string.Format("Spieleanfrage zum Ablehnen von UserId {0} an UserId {1} wurde bereits abgelehnt.", id.ToString(), requestedUser.Id.ToString()));
                return BadRequest(new Classes.Responses.StdResponse(false, e.Message));
            }
            catch (Exception e)
            {
                _Logger.LogError(e, string.Format("Unerwarteter Fehler beim Ablehnen der Spieleanfrage von UserId {0} and UserId {1}.", id.ToString(), requestedUser.Id.ToString()));

                return BadRequest(new Classes.Responses.StdResponse(false, "Unerwarteter Fehler beim Ablehnen der Spieleanfrage."));
            }

            _Logger.LogInformation(string.Format("Spieleanfrage von UserId {0} durch UserId {1} abgelehnt.", id.ToString(), requestedUser.Id.ToString()));

            return Ok(new Classes.Responses.StdResponse(true, "Spielanfrage abgelehnt."));

        }

        /// <summary>
        ///   Liefert einen Nutzer anhand seiner Id
        ///   Achtung => Adminrechte notwendig
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns>Nutzer</returns>
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Model.User requestedUser = _userDatabaseService.GetUser(id);

            if (!_IsAccessAllowed(requestedUser, HttpContext))
            {
                return NotFound();
            }            

            return Ok(requestedUser);
        }

        /// <summary>
        ///   Liefert das Nutzerbild eines Nutzers
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns>BASE64-kodiertes Bild</returns>
        [Authorize]
        [HttpGet("{id}/Picture")]
        public IActionResult GetPicture(int id)
        {
            if (!_IsAccessAllowed(id, HttpContext))
            {
                return NotFound();
            }

            return Ok(_userDatabaseService.GetUserPicture(id));
        }

        private IActionResult _GetPlayRequest(Model.User user, bool outgoing)
        {
            List<Classes.UserPlayingPair> requests = new List<Classes.UserPlayingPair>();           

            try
            {
                requests = _sessionService.GetPlayRequest(user, outgoing);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, string.Format("Fehler bei Abfrage der Spieleanfragen für UserId {0}.", user.Id));
                return BadRequest(e.Message);
            }


            return Ok(requests);
        }

        private bool _IsAccessAllowed(int requestedUserId, HttpContext context)
        {
            return _IsAccessAllowed(_userDatabaseService.GetUser(requestedUserId), context);
        }

        private bool _IsAccessAllowed(Model.User requestUser, HttpContext context)
        {
            Model.User LoggedOnUser = _userDatabaseService.GetUser(Classes.Util.GetUsernameFromToken(context));

            if (requestUser.Id != LoggedOnUser.Id && !LoggedOnUser.IsAdmin)
            {
                return false;
            }

            return true;
        }

    }
}
