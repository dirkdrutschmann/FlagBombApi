using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Secret { get; set; }

        public string Email { get; set; }


        /// <summary>
        ///   Führt die Authentifizierung eines Nutzers durch
        /// </summary>
        /// <param name="login">Usermodel mit Nutzername und Passwort</param>
        /// <returns>Instanz eines Nutzers</returns>
        public static User Authenticate(User login)
        {
            User user = null;

            if (login.Username == "frantic")
            {
                user = new User()
                {
                    Username = "frantic",
                    Email = "frantic88@gmx.de",
                    Secret = "ThisismySecretKey"
                };
            }

            return user;
        }
    }
}
