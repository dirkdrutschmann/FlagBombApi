using APIPacBomb.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    public class UserDatabaseService : DatabaseService, Interfaces.IUserDatabaseService
    {
        /// <summary>
        ///   Erstellt eine Instanz des Datenbankdienstes
        /// </summary>
        /// <param name="user">DB-Nutzername</param>
        /// <param name="pass">DB-Passwort</param>
        /// <param name="server">DB-Server</param>
        /// <param name="database">DB-Name</param>
        public UserDatabaseService(string user, string pass, string server, string database) : base(user, pass, server, database)
        { }

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankservices
        /// </summary>
        /// <param name="connectionString">Verbindungszeichenkette</param>
        public UserDatabaseService(string connectionString) : base(connectionString)
        { }

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankservices
        /// </summary>
        /// <param name="connection">Bestehende Verbindung</param>
        public UserDatabaseService(MySqlConnection connection): base(connection)
        { }

        /// <summary>
        ///   Liefert <code>true</code> zurück, wenn die übergebene Mail bereits in der users-Tabelle existiert,
        ///   sonst <code>false</code>
        /// </summary>
        /// <param name="mail">E-Mailadresse</param>
        /// <returns><code>true</code>/<code>false</code></returns>
        public bool ExistsMail(string mail)
        {
            string query = "select 1 from pb_users where email=@mail";

            List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@mail", mail)
            };

            if (_ExecuteQuery(query, param).Count() > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Liefert <code>true</code> zurück, wenn der übergebene Nutzername bereits in der users-Tabelle existiert,
        ///   sonst <code>false</code>
        /// </summary>
        /// <param name="username">Nutzername</param>
        /// <returns><code>true</code>/<code>false</code></returns>
        public bool ExistsUsername(string username)
        {
            string query = "select 1 from pb_users where username=@username";

            List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@username", username)
            };

            if (_ExecuteQuery(query, param).Count() > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Legt einen neuen Nutzer an und liefert diesen als Instanz der User-Klasse zurück
        /// </summary>
        /// <param name="user">Userinformationen</param>
        /// <returns>Neuer Nutzer</returns>
        public User RegisterUser(User user)
        {
            string cmd = "insert into pb_users(username, email, prename, lastname, registration_on, password, secret) values (@username, @email, @prename, @lastname, @registration_on, @password, @secret)";

            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@username", user.Username),
                new KeyValuePair<string, string>("@email", user.Email),
                new KeyValuePair<string, string>("@prename", user.Prename),
                new KeyValuePair<string, string>("@lastname", user.Lastname),
                new KeyValuePair<string, string>("@registration_on", user.RegistrationOn.ToString("yyyy-MM-dd HH:mm:ss")),
                new KeyValuePair<string, string>("@password", user.Password),
                new KeyValuePair<string, string>("@secret", user.Secret)
            };

            _ExecuteNonQuery(cmd, paras);

            return GetUser(user.Username);
        }

        /// <summary>
        ///   Liefert den Nutzer mit entsprechenden Nutzernamen oder E-Mailadresse zurück
        /// </summary>
        /// <param name="usernameOrMail">Nutzername oder E-Mailadresse</param>
        /// <returns>Ermittelter Nutzer oder null, wenn kein Nutzer gefunden wurde</returns>
        public User GetUser(string usernameOrMail)
        {
            string cmd = "select id, username, email, prename, lastname, registration_on, last_logon from pb_users where username = @username or email = @email";
            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@username", usernameOrMail),
                new KeyValuePair<string, string>("@email", usernameOrMail)
            };

            IEnumerable<IDataRecord> records = _ExecuteQuery(cmd, paras);

            User user = null;

            if (records.Count() > 0)
            {
                user = new User();

                foreach (IDataRecord record in records)
                {
                    user.Id = Convert.ToInt32(record.GetValue(0));
                    user.Username = record.GetString(1);
                    user.Email = record.GetString(2);
                    user.Prename = record.GetString(3);
                    user.Lastname = record.GetString(4);
                    user.RegistrationOn = record.GetDateTime(5);
                    user.LastLogon = record.IsDBNull(6) ? DateTime.Now : record.GetDateTime(6);


                    break;
                }
            }

            return user;
        }

        /// <summary>
        ///   Ändert Nutzerinformationen
        /// </summary>
        /// <param name="user"></param>
        public void SetUser(User user)
        {
            User compUser = GetUser(user.Username);

            string cmd = "update pb_users set {0} where id = @id";

            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@id", user.Id.ToString())
            };

            string set = string.Empty;

            if (!user.Username.Equals(compUser.Username))
            {
                set = "username = @username";
                paras.Add(new KeyValuePair<string, string>("@username", user.Username));
            }

            if (!user.Email.Equals(compUser.Email))
            {
                set = (!string.IsNullOrEmpty(set) ? ", " : "") + "email = @email";
                paras.Add(new KeyValuePair<string, string>("@email", user.Email));
            }

            if (!user.Prename.Equals(compUser.Prename))
            {
                set = (!string.IsNullOrEmpty(set) ? ", " : "") + "prename = @prename";
                paras.Add(new KeyValuePair<string, string>("@prename", user.Prename));
            }

            if (!user.Lastname.Equals(compUser.Lastname))
            {
                set = (!string.IsNullOrEmpty(set) ? ", " : "") + "lastname = @lastname";
                paras.Add(new KeyValuePair<string, string>("@lastname", user.Lastname));
            }

            if (!user.LastLogon.Equals(compUser.LastLogon))
            {
                set = (!string.IsNullOrEmpty(set) ? ", " : "") + "last_logon = @last_logon";
                paras.Add(new KeyValuePair<string, string>("@last_logon", user.LastLogon.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            if (!string.IsNullOrEmpty(set))
            {
                _ExecuteNonQuery(string.Format(cmd, set), paras);
            }            
        }

        /// <summary>
        ///   Führt die Authentifizierung des Nutzers durch
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public User Authenticate(User user)
        {
            string cmd = "select secret, password from pb_users where username = @username or email = @email";
            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@username", string.IsNullOrEmpty(user.Username) ? "" : user.Username),
                new KeyValuePair<string, string>("@email", string.IsNullOrEmpty(user.Email) ? "" : user.Email)                
            };            

            IEnumerable<IDataRecord> records = _ExecuteQuery(cmd, paras);

            User regUser = null;

            if (records.Count() > 0)
            {
                bool found = false;

                foreach (IDataRecord record in records)
                {
                    if (User.GeneratePasswordHash(user.Password, record.GetString(0)) == record.GetString(1))
                    {
                        found = true;                        
                    }
                }

                if (found)
                {
                    regUser = GetUser(string.IsNullOrEmpty(user.Username) ? user.Email : user.Username);
                }
                
            }

            return regUser;
        }
    }
}
