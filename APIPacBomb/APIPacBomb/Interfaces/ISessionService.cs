using APIPacBomb.Model;
using System.Collections.Generic;

namespace APIPacBomb.Interfaces
{
    public interface ISessionService
    {
        void AddLoggedinUser(User user);
        
        bool RemoveLoggedinUser(User user);
        
        List<User> GetLoggedInUsers();

        void SendPlayRequest(User user, int requestedUserId);

        List<Classes.UserPlayingPair> GetPlayRequest(User user, bool outgoing = false);

        void AcceptPlayRequest(User user, int requestingUserId);

        void RejectPlayRequest(User user, int requestingUserId);

        User GetUser(string username);
        
        User GetUser(int userId);
    }
}