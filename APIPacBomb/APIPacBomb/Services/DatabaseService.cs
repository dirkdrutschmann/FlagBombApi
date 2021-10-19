using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    public class DatabaseService
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
                "SERVER={0};DATABASE={1};UID={2};PASSWORD={3};SSL Mode=None;convert zero datetime=True",
                server,
                database,
                user,
                pass
            );

            _dbConnection = new MySqlConnection(connectionString);
        }

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankservices
        /// </summary>
        /// <param name="connectionString">Verbindungszeichenkette</param>
        public DatabaseService(string connectionString)
        {
            _dbConnection = new MySqlConnection(connectionString);
        }

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankservices
        /// </summary>
        /// <param name="connection">Bestehende Verbindung</param>
        public DatabaseService(MySqlConnection connection)
        {
            _dbConnection = connection;
        }

        /// <summary>
        ///   Liefert die geöffnete Datenbankverbindung zurück
        /// </summary>
        /// <returns>Datenbankverbindung</returns>
        protected MySqlConnection _GetConnection()
        {
            if (_dbConnection.State == System.Data.ConnectionState.Closed || _dbConnection.State == System.Data.ConnectionState.Broken)
            {
                _dbConnection.Open();
            }

            return _dbConnection;
        }

        /// <summary>
        ///   Liefert das Result-Set als abzählbare Recordsammlung
        /// </summary>
        /// <param name="query">Abfrage</param>
        /// <param name="param">Abfrageparameter</param>
        /// <returns>Result-Set</returns>
        protected IEnumerable<IDataRecord> _ExecuteQuery(string query, List<KeyValuePair<string, string>> param)
        {

            using (MySqlCommand cmd = new MySqlCommand(query, _GetConnection()))
            {
                if (param.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in param)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }

                    cmd.Prepare();
                }


                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return reader;
                    }
                }

            }

        }

        /// <summary>
        ///   Führt ein Nichtabfrage-Statement aus
        /// </summary>
        /// <param name="cmdText">Kommando</param>
        /// <param name="param">Kommandoparameter</param>
        protected void _ExecuteNonQuery(string cmdText, List<KeyValuePair<string, string>> param)
        {
            using (MySqlCommand cmd = new MySqlCommand(cmdText, _GetConnection()))
            {
                if (param.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in param)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }

                    cmd.Prepare();
                }


                cmd.ExecuteNonQuery();
            }
        }
    }
}
