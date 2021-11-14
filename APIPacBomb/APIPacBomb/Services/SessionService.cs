using APIPacBomb.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    /// <summary>
    ///   Service für Handling der aktiv angemeldeten Nutzer
    /// </summary>
    public class SessionService : Interfaces.ISessionService
    {    
        ILogger<SessionService> _logger;

        private List<Model.User> _LoggedinUsers = new List<Model.User>();

        private List<UserPlayingPair> _userPlayingPairs = new List<UserPlayingPair>();

        private System.Threading.Thread _ExpiryThread;

        /// <summary>
        ///   Wird ausgelöst, wenn alle Spielpartner verbunden sind
        /// </summary>
        public event EventHandler<UserPlayingPair> AllPartnersConnected;

        /// <summary>
        ///   Erstellt eine Instanz der SessionService-Klasse
        /// </summary>
        /// <param name="logger">Logging-Instanz</param>
        public SessionService(ILogger<SessionService> logger)
        {
            _logger = logger;

            _logger.LogInformation("SessionService started.");
            _ExpiryThread = new System.Threading.Thread(new System.Threading.ThreadStart(_ExpiryThreadHandle));
            _ExpiryThread.Start();
        }

        /// <summary>
        ///   Destruktor
        /// </summary>
        ~SessionService()
        {
            if (_ExpiryThread.IsAlive && _ExpiryThread.ThreadState != System.Threading.ThreadState.Aborted)
            {
                _ExpiryThread.Abort();
            }            
        }

        /// <summary>
        ///   Fügt einen Nutzer in die Liste der aktiven Nutzer hinzu
        /// </summary>
        /// <param name="user">Angemeldeter Nutzer</param>
        public void AddLoggedinUser(Model.User user)
        {
            if (_GetIndexOf(user)== -1)
            {
                _LoggedinUsers.Add(user);
            }            

            _logger.LogInformation(string.Format("User \"{0}\" ({1}) logged in.", user.Username, user.Id.ToString()));
        }

        /// <summary>
        ///   Löscht einen Nutzer aus der Liste der angemeldeten Nutzer und löscht alle ein- und ausgehenden Spieleanfragen
        /// </summary>
        /// <param name="user">Angemeldeter Nutzer</param>
        /// <returns>true, wenn Lösung erfolgreich war, sonst false</returns>
        public bool RemoveLoggedinUser(Model.User user)
        {
            int i = _GetIndexOf(user);

            if (i < 0)
            {
                _logger.LogInformation(string.Format("User \"{0}\" ({1}) not found.", user.Username, user.Id.ToString()));
                return false;
            }

            // Alle Anfragen entfernen
            List<UserPlayingPair> playingPairs = GetPlayRequest(user, false);
            playingPairs = playingPairs.Concat(GetPlayRequest(user, true)).ToList();

            foreach (UserPlayingPair pair in playingPairs)
            {
                _userPlayingPairs.Remove(pair);
            }

            _LoggedinUsers.RemoveAt(i);
            _logger.LogInformation(string.Format("User \"{0}\" ({1}) logged out.", user.Username, user.Id.ToString()));

            return true;
        }

        /// <summary>
        ///   Liefert die Liste der angemeldeten Nutzer
        /// </summary>
        /// <returns>Liste der angemeldeten Nutzer</returns>
        public List<Model.User> GetLoggedInUsers()
        {
            return _LoggedinUsers;
        }

        /// <summary>
        ///   Sendet eine Spielanfrage an einen Nutzer
        /// </summary>
        /// <param name="user">Anfragender Nutzer</param>
        /// <param name="requestedUserId">Angefragter Nutzer</param>
        /// <param name="context">HTTP-Context der Anfrage</param>
        public void SendPlayRequest(Model.User user, int requestedUserId, Model.Map.Grid mapConfig, HttpContext context)
        {
            // Check if requesting User is logged in
            if (_GetIndexOf(user) < 0)
            {
                throw new Classes.Exceptions.NotLoggedInException("Anfragender Nutzer ist nicht angemeldet.");
            }

            int indexOfRequestedUser = _GetIndexOf(requestedUserId);

            // Check if requested User is logged in
            if (indexOfRequestedUser < 0)
            {
                throw new Classes.Exceptions.NotLoggedInException("Angefragter Nutzer ist nicht angemeldet.");
            }

            // Check if request already exists
            if (_GetIndexOfPlayingPair(user, GetUser(requestedUserId)) >= 0)
            {
                throw new Classes.Exceptions.PlayRequestAlreadyExistsException("Anfrage bereits an UserId " + requestedUserId + " gesendet.");
            }

            _userPlayingPairs.Add(
                new UserPlayingPair(user, _LoggedinUsers[indexOfRequestedUser])
                {
                    RequestedIP = context.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                    Map = mapConfig
                }
            );

            _logger.LogInformation(string.Format("Spieleanfrage von UserId {0} an UserId {1} gestellt.", user.Id.ToString(), requestedUserId.ToString()));
        }

        /// <summary>
        ///   Liefert eine Liste von Anfragen, die einem Nutzer gestellt wurden oder die dieser erhalten hat
        /// </summary>
        /// <param name="user">Nutzer für den die Liste erzeugt werden soll</param>
        /// <param name="outgoing">false, wenn die eingegangen Anfragen ermittelt werden sollen. true, wenn die ausgehenden Anfragen ermittelt werden</param>
        /// <returns>Liste der Anfragen</returns>
        public List<UserPlayingPair> GetPlayRequest(Model.User user, bool outgoing = false)
        {
            // Check if requesting User is logged in
            if (_GetIndexOf(user) < 0)
            {
                throw new Classes.Exceptions.NotLoggedInException("Nutzer ist nicht angemeldet.");
            }

            List<UserPlayingPair> requests = new List<UserPlayingPair>();

            foreach (UserPlayingPair request in _userPlayingPairs)
            {
                if 
                (
                    ((!outgoing && request.RequestedUser.Id == user.Id) || (outgoing && request.RequestingUser.Id == user.Id)) &&
                    (request.Status == UserPlayingPair.PlayingStatus.REQUESTED || request.Status == UserPlayingPair.PlayingStatus.ACCEPTED)
                )
                {
                    requests.Add(request);
                }
            }

            return requests;
        }

        /// <summary>
        ///   Akzeptiert eine Spieleanfrage
        /// </summary>
        /// <param name="user">Akzeptierender Nutzer</param>
        /// <param name="requestingUserId">UserId des anfragenden Nutzers</param>
        public void AcceptPlayRequest(Model.User user, int requestingUserId, HttpContext context)
        {
            int index = _GetIndexOfPlayingPair(GetUser(requestingUserId), user);

            _userPlayingPairs[index].RequestingIP = context.Connection.RemoteIpAddress.MapToIPv4().ToString();

            _SetStatus(index, UserPlayingPair.PlayingStatus.ACCEPTED);
        }

        /// <summary>
        ///   Lehnt eine Spieleanfrage ab
        /// </summary>
        /// <param name="user">Ablehender Nutzer</param>
        /// <param name="requestingUserId">UserId des anfragenden Nutzers</param>
        public void RejectPlayRequest(Model.User user, int requestingUserId)
        {
            _SetStatus(_GetIndexOfPlayingPair(GetUser(requestingUserId), user), UserPlayingPair.PlayingStatus.REJECTED);
        }

        /// <summary>
        ///   Liefert den Nutzer anhand seines Nutzernamens zurück
        /// </summary>
        /// <param name="username">Nutzername</param>
        /// <returns>Instanz der Userklasse</returns>
        public Model.User GetUser(string username)
        {
            return _LoggedinUsers.Find(u => u.Username == username);
        }

        /// <summary>
        ///   Liefert den Nutzer anhand seiner UserId
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <returns>Instanz der Userklsse</returns>
        public Model.User GetUser(int userId)
        {
            return _LoggedinUsers[_GetIndexOf(userId)];
        }

        /// <summary>
        ///   Setzt die Websocket-Instanz für einen Spieler eines Spielerpaares
        /// </summary>
        /// <param name="socket">WebSocket</param>
        /// <param name="playingPairId">Id des Spielerpaars</param>
        /// <param name="userId">UserId des Spielers</param>
        /// <param name="context">HTTP-Context</param>
        public void SetWebSocket(WebSocket socket, string playingPairId, int userId, HttpContext context)
        {
            int i = _GetIndexOfPlayingPair(playingPairId);

            if (i < 0)
            {
                throw new Classes.Exceptions.PlayingPairNotFoundException("Spielpaarung nicht gefunden.");
            }

            string remoteIp = context.Connection.RemoteIpAddress.MapToIPv4().ToString();

            if (_userPlayingPairs[i].RequestedIP != remoteIp && _userPlayingPairs[i].RequestingIP != remoteIp)
            {
                _logger.LogWarning(string.Format("IP {0} versucht auf andere Spielpaarung zuzugreifen.", remoteIp));
                throw new Classes.Exceptions.PlayingPairNotFoundException("Spielpaarung nicht gefunden");
            }

            if (_userPlayingPairs[i].RequestedUser.Id == userId && (_userPlayingPairs[i].RequestedUser.WebSocket == null || _userPlayingPairs[i].RequestedUser.WebSocket.State != WebSocketState.Open))
            {
                _userPlayingPairs[i].RequestedUser.WebSocket = socket;

                if (_userPlayingPairs[i].RequestingUser.WebSocket != null &&_userPlayingPairs[i].RequestingUser.WebSocket.State == WebSocketState.Open)
                {
                    AllPartnersConnected?.Invoke(this, _userPlayingPairs[i]);
                }
            }
            else if (_userPlayingPairs[i].RequestingUser.Id == userId && (_userPlayingPairs[i].RequestingUser.WebSocket == null || _userPlayingPairs[i].RequestingUser.WebSocket.State != WebSocketState.Open))
            {
                _userPlayingPairs[i].RequestingUser.WebSocket = socket;

                if (_userPlayingPairs[i].RequestedUser.WebSocket != null &&_userPlayingPairs[i].RequestedUser.WebSocket.State == WebSocketState.Open)
                {
                    AllPartnersConnected?.Invoke(this, _userPlayingPairs[i]);
                }
            }
        }

        /// <summary>
        ///   Liefert den WebSocket des Partners der Spielpaarung
        /// </summary>
        /// <param name="playingPairId">Id des Spielerpaars</param>
        /// <param name="userId">UserId des Spieler</param>
        /// <returns>WebSocket des Partners von userId</returns>
        public WebSocket GetPartnerWebSocket(string playingPairId, int userId)
        {
            int i = _GetIndexOfPlayingPair(playingPairId);

            if (i < 0)
            {
                throw new Classes.Exceptions.PlayingPairNotFoundException("Spielerpaarung nicht gefunden.");
            }

            if (_userPlayingPairs[i].RequestedUser.Id == userId)
            {
                return _userPlayingPairs[i].RequestingUser.WebSocket;
            }
            else
            {
                return _userPlayingPairs[i].RequestedUser.WebSocket;
            }
        }

        /// <summary>
        ///   Liefert eine Spielpaarung zurück
        /// </summary>
        /// <param name="playingPairId">Id des Spielerpaars</param>
        /// <returns>Spielerpaarung</returns>
        public UserPlayingPair GetUserPlayingPair(string playingPairId)
        {
            int i = _GetIndexOfPlayingPair(playingPairId);

            if (i < 0)
            {
                throw new Classes.Exceptions.PlayingPairNotFoundException("Spielpaarung nicht gefunden");
            }

            return _userPlayingPairs[i];
        }

        /// <summary>
        ///   Aktualisiert eine Spielpaarung
        /// </summary>
        /// <param name="pair">Instanz der Spielpaarung</param>
        public void UpdatePlayingPair(UserPlayingPair pair)
        {
            int i = _GetIndexOfPlayingPair(pair.Id.ToString());

            if (i < 0)
            {
                throw new Classes.Exceptions.PlayingPairNotFoundException(string.Format("Spielpaarung {0} nicht gefunden.", pair.Id.ToString()));
            }

            _userPlayingPairs[i] = pair;
        }

        /// <summary>
        ///   Liefert den Index aus den LoggedInUsers
        /// </summary>
        /// <param name="user">Zu suchender Nutzer</param>
        /// <returns>0-basierender Index, wenn nicht gefunden -1</returns>
        private int _GetIndexOf(Model.User user)
        {
            return _GetIndexOf(user.Id);
        }

        /// <summary>
        ///   Liefert den Index aus den LoggedInUsers
        /// </summary>
        /// <param name="userId">Zu suchende UserId</param>
        /// <returns>0-basierender Index, wenn nicht gefunden -1</returns>
        private int _GetIndexOf(int userId)
        {
            return _LoggedinUsers.FindIndex(u => u.Id == userId);
        }

        /// <summary>
        ///   Liefert den Index aus den UserPlayingPairs
        /// </summary>
        /// <param name="requestingUser">Anfragender Nutzer</param>
        /// <param name="requestedUser">Angefragter Nutzer</param>
        /// <returns>0-basierender Index, wenn nicht gefunden -1</returns>
        private int _GetIndexOfPlayingPair(Model.User requestingUser, Model.User requestedUser)
        {
            return _userPlayingPairs.FindIndex(pair => pair.RequestingUser.Id == requestingUser.Id && pair.RequestedUser.Id == requestedUser.Id);
        }

        /// <summary>
        ///   Liefert den Index aus den UserPlayingPairs
        /// </summary>
        /// <param name="id">Id des PlayingPairs</param>
        /// <returns></returns>
        private int _GetIndexOfPlayingPair(string id)
        {
            return _userPlayingPairs.FindIndex(pair => pair.Id.ToString() == id);
        }

        /// <summary>
        ///   Setzt den Status eines Spielerpaares
        /// </summary>
        /// <param name="index">Index für UserPlayingPairs</param>
        /// <param name="status">Neuer Status</param>
        private void _SetStatus(int index, UserPlayingPair.PlayingStatus status)
        {
            if (index < 0)
            {
                throw new Classes.Exceptions.PlayingPairNotFoundException("Spielanfrage konnte nicht gefunden werden.");
            }

            UserPlayingPair pair = _userPlayingPairs[index];

            if (pair.Status == status)
            {
                throw new Classes.Exceptions.StateAlreadySetException(
                    string.Format(
                        "Spieleanfrage von {0} an {1} hat bereits den Status {2}.",
                        pair.RequestingUser.Username,
                        pair.RequestedUser.Username,
                        status.ToString()
                    )
                );
            }

            _userPlayingPairs[index].Status = status;
        }

        /// <summary>
        ///   Löscht alle Anfragen, die abgelehnt oder gestellt wurden und älter als 30 Sekunden sind
        /// </summary>
        private async void _ExpiryThreadHandle()
        {
            UserPlayingPair pair;

            while (true)
            {
                for (int i = 0; i < _userPlayingPairs.Count; i++)
                {
                    pair = _userPlayingPairs[i];

                    if (pair.Status == UserPlayingPair.PlayingStatus.IN_GAME)
                    {
                        continue;
                    }

                    if ((DateTime.Now - pair.RequestTime).TotalSeconds < 120 && pair.Status != UserPlayingPair.PlayingStatus.GAME_OVER)
                    {
                        continue;
                    }

                    if (_userPlayingPairs[i].RequestedUser.WebSocket != null && !_userPlayingPairs[i].RequestedUser.WebSocket.CloseStatus.HasValue)
                    {
                        await _userPlayingPairs[i].RequestedUser.WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Expired", System.Threading.CancellationToken.None);
                    }                    
                    
                    _userPlayingPairs[i].RequestedUser.WebSocket?.Dispose();
                    _userPlayingPairs[i].RequestedUser.WebSocket = null;

                    if (_userPlayingPairs[i].RequestingUser.WebSocket != null && !_userPlayingPairs[i].RequestingUser.WebSocket.CloseStatus.HasValue)
                    {
                        await _userPlayingPairs[i].RequestingUser.WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Expired", System.Threading.CancellationToken.None);
                    }                    

                    _userPlayingPairs[i].RequestingUser.WebSocket?.Dispose();
                    _userPlayingPairs[i].RequestingUser.WebSocket = null;


                    _userPlayingPairs.RemoveAt(i);

                    _logger.LogInformation(
                        string.Format(
                            "Spieleanfrage von {0} von UserId {1} an UserId {2} gelöscht.",
                            pair.RequestTime.ToString("dd.MM.yyyy HH:mm:ss"),
                            pair.RequestingUser.Id.ToString(),
                            pair.RequestedUser.Id.ToString()
                        )
                    );

                }

                System.Threading.Thread.Sleep(1000);
            }
        }


    }
}