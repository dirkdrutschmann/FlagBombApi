using APIPacBomb.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace APIPacBomb.Interfaces
{
    public interface ISessionService
    {
        public event EventHandler<Classes.UserPlayingPair> AllPartnersConnected;

        void AddLoggedinUser(User user);

        bool RemoveLoggedinUser(User user);

        List<User> GetLoggedInUsers();

        void SendPlayRequest(User user, int requestedUserId, Model.Map.Grid mapConfig, HttpContext context);

        List<Classes.UserPlayingPair> GetPlayRequest(User user, bool outgoing = false);

        void AcceptPlayRequest(User user, int requestingUserId, HttpContext context);

        void RejectPlayRequest(User user, int requestingUserId);

        User GetUser(string username);
        
        User GetUser(int userId);

        void SetWebSocket(System.Net.WebSockets.WebSocket socket, string playingPairId, int userId, HttpContext context);

        System.Net.WebSockets.WebSocket GetPartnerWebSocket(string playingPairId, int userId);

        System.Threading.Tasks.Task SendMessageToAllPlayers(string playingPairId, string message);

        Classes.UserPlayingPair GetUserPlayingPair(string playingPairId);

        void UpdatePlayingPair(Classes.UserPlayingPair pair);
    }
}