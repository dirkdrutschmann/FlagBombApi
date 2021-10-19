using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Interfaces
{
    public interface IUserDatabaseService
    {
        public bool ExistsUsername(string username);

        public bool ExistsMail(string mail);

        public Model.User RegisterUser(Model.User user);

        public Model.User GetUser(string usernameOrMail);

        public void SetUser(Model.User user);

        public Model.User Authenticate(Model.User user);
    }
}
