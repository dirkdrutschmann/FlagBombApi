using APIPacBomb.Model;
using System.Collections.Generic;

namespace APIPacBomb.Interfaces
{
    public interface ISessionService
    {
        void AddLoggedinUser(User user);
        bool RemoveLoggedinUser(User user);
        List<User> GetLoggedInUsers();
    }
}