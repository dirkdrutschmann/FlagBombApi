using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    public class DatabaseService : Interfaces.IDatabaseService
    {
        /// <summary>
        ///   Datenbankverbindung
        /// </summary>
        private MySqlConnection _dbConnection; 

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankdienstes
        /// </summary>
        /// <param name="user">DB-Nutzername</param>
        /// <param name="pass">DB-Passwort</param>
        /// <param name="server">DB-Server</param>
        /// <param name="database">DB-Name</param>
        public DatabaseService(string user, string pass, string server, string database)
        {
            string connectionString = string.Format(
                "SERVER={0};DATABASE={1};UID={2};PASSWORD={3}; SSL Mode=None",
                server,
                database,
                user,
                pass
            );

            _dbConnection = new MySqlConnection(connectionString);
        }

        /// <summary>
        ///   Liefert die geöffnete Datenbankverbindung zurück
        /// </summary>
        /// <returns>Datenbankverbindung</returns>
        private MySqlConnection _GetCnnection()
        {
            if (_dbConnection.State == System.Data.ConnectionState.Closed || _dbConnection.State == System.Data.ConnectionState.Broken)
            {
                _dbConnection.Open();
            }

            return _dbConnection;
        }
    }
}
