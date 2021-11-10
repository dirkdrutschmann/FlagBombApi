using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    public class SessionService : Interfaces.ISessionService
    {

        ILogger<SessionService> _logger;

        private List<Model.User> _LoggedinUsers = new List<Model.User>();

        public SessionService(ILogger<SessionService> logger)
        {
            _logger = logger;

            _logger.LogInformation("SessionService started.");
        }

        public void AddLoggedinUser(Model.User user)
        {
            if (_GetIndexOf(user)== -1)
            {
                _LoggedinUsers.Add(user);
            }            

            _logger.LogInformation(string.Format("User \"{0}\" ({1}) logged in.", user.Username, user.Id.ToString()));
        }

        public bool RemoveLoggedinUser(Model.User user)
        {
            int i = _GetIndexOf(user);

            if (i < 0)
            {
                _logger.LogInformation(string.Format("User \"{0}\" ({1}) not found.", user.Username, user.Id.ToString()));
                return false;
            }

            _logger.LogInformation(string.Format("User \"{0}\" ({1}) logged out.", user.Username, user.Id.ToString()));
            _LoggedinUsers.RemoveAt(i);

            return true;
        }

        public List<Model.User> GetLoggedInUsers()
        {
            return _LoggedinUsers;
        }

        private int _GetIndexOf(Model.User user)
        {
            return _LoggedinUsers.FindIndex(u => u.Id == user.Id);
        }

    }
}