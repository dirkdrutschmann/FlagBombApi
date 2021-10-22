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

        public User GetUser(int id)
        {
            string cmd = "select id, " +
                         "       username, " +
                         "       email, " +
                         "       prename, " +
                         "       lastname, " +
                         "       registration_on, " +
                         "       last_logon, " +
                         "       is_admin " +
                         "from  pb_users " +
                         "where id = @id";

            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@id", id.ToString())
            };

            return _GenerateUserObject(_ExecuteQuery(cmd, paras));            
        }

        /// <summary>
        ///   Liefert den Nutzer mit entsprechenden Nutzernamen oder E-Mailadresse zurück
        /// </summary>
        /// <param name="usernameOrMail">Nutzername oder E-Mailadresse</param>
        /// <returns>Ermittelter Nutzer oder null, wenn kein Nutzer gefunden wurde</returns>
        public User GetUser(string usernameOrMail)
        {
            string cmd = "select id, " +
                         "       username, " +
                         "       email, " +
                         "       prename, " +
                         "       lastname, " +
                         "       registration_on, " +
                         "       last_logon, " +
                         "       is_admin " +
                         "from  pb_users " +
                         "where username = @username " +
                         "or    email = @email";

            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@username", usernameOrMail),
                new KeyValuePair<string, string>("@email", usernameOrMail)
            };

            return _GenerateUserObject(_ExecuteQuery(cmd, paras));
        }

        /// <summary>
        ///   Liefert das Nutzerbild als Base64-kodierter String
        /// </summary>
        /// <param name="id">User-Id</param>
        /// <returns>Base64-kodierter String</returns>
        public string GetUserPicture(int id)
        {
            string cmd = "select length(picture) as file_size, " +
                         "       picture " +
                         "from  pb_users " +
                         "where id = @id";

            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@id", id.ToString())
            };

            IEnumerable<IDataRecord> records = _ExecuteQuery(cmd, paras);

            string pictureBase64 = string.Empty;

            foreach (IDataRecord record in records)
            {
                int fileSize = record.GetInt32(record.GetOrdinal("file_size"));
                byte[] rawData = new byte[fileSize];
                record.GetBytes(record.GetOrdinal("picture"), 0, rawData, 0, fileSize);
                pictureBase64 = Convert.ToBase64String(rawData);

                break;
            }

            return pictureBase64;

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
            string cmd = "select id, secret, password from pb_users where username = @username or email = @email";
            List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("@username", string.IsNullOrEmpty(user.Username) ? "" : user.Username),
                new KeyValuePair<string, string>("@email", string.IsNullOrEmpty(user.Email) ? "" : user.Email)                
            };            

            IEnumerable<IDataRecord> records = _ExecuteQuery(cmd, paras);

            User regUser = null;

            if (records.Count() > 0)
            {
                int id = -1;

                foreach (IDataRecord record in records)
                {
                    if (User.GeneratePasswordHash(user.Password, record.GetString(record.GetOrdinal("secret"))) == record.GetString(record.GetOrdinal("password")))
                    {
                        id = record.GetInt32(record.GetOrdinal("id"));                        
                    }
                }

                if (id >= 0)
                {
                    regUser = GetUser(id);
                }                
            }

            return regUser;
        }

        /// <summary>
        ///   Liefert anhand des übergebene Record ein Nutzerobjekt
        /// </summary>
        /// <param name="records">Resultset</param>
        /// <returns>Instanz eines Nutzers</returns>
        private User _GenerateUserObject(IEnumerable<IDataRecord> records)
        {
            User user = null;

            if (records.Count() > 0)
            {
                user = new User();

                foreach (IDataRecord record in records)
                {
                    user.Id = Convert.ToInt32(record.GetValue(record.GetOrdinal("id")));
                    user.Username = record.GetString(record.GetOrdinal("username"));
                    user.Email = record.GetString(record.GetOrdinal("email"));
                    user.Prename = record.GetString(record.GetOrdinal("prename"));
                    user.Lastname = record.GetString(record.GetOrdinal("lastname"));
                    user.RegistrationOn = record.GetDateTime(record.GetOrdinal("registration_on"));
                    user.LastLogon = record.IsDBNull(6) ? DateTime.Now : record.GetDateTime(record.GetOrdinal("last_logon"));
                    user.IsAdmin = record.GetInt16(record.GetOrdinal("is_admin")) == 1 ? true : false;                    

                    break;
                }
            }

            return user;
        }
    }
}
