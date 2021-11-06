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
        ///   Datenbankcommand
        /// </summary>
        private MySqlCommand _dbCommand;

        /// <summary>
        ///   Verbindungszeichenkette
        /// </summary>
        private readonly string _ConnectionString;

        /// <summary>
        ///   Verbindungsversuche
        /// </summary>
        private int _CurrentConnectionRetries { get; set; }        

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankdienstes
        /// </summary>
        /// <param name="user">DB-Nutzername</param>
        /// <param name="pass">DB-Passwort</param>
        /// <param name="server">DB-Server</param>
        /// <param name="database">DB-Name</param>
        public DatabaseService(string user, string pass, string server, string database)
        {
            _ConnectionString = string.Format(
                "SERVER={0};DATABASE={1};UID={2};PASSWORD={3};SSL Mode=None;convert zero datetime=True",
                server,
                database,
                user,
                pass
            );

            _dbConnection = new MySqlConnection(_ConnectionString);
            _dbCommand = new MySqlCommand();
            _CurrentConnectionRetries = 0;
        }

        /// <summary>
        ///   Liefert die geöffnete Datenbankverbindung zurück
        /// </summary>
        /// <returns>Datenbankverbindung</returns>
        protected MySqlConnection _SetConnection()
        {
            if (_dbConnection.State == ConnectionState.Closed || _dbConnection.State == ConnectionState.Broken)
            {
                _dbConnection.Open();
            }

            try
            {
                _dbCommand.CommandText = "select 1 from dual";
                _dbCommand.Connection = _dbConnection;
                _dbCommand.ExecuteScalar();
            }
            catch
            {
                _CurrentConnectionRetries++;
                _dbConnection = new MySqlConnection(_ConnectionString);

                _dbCommand = new MySqlCommand();
                _dbCommand.Connection = _dbConnection;


                if (_CurrentConnectionRetries <= 5)
                {                    
                    return _SetConnection();
                }

                return null;

            }

            _CurrentConnectionRetries = 0;
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
            
            _SetConnection();
            _dbCommand.CommandText = query;
            _dbCommand.Parameters.Clear();

            if (param.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in param)
                {
                    _dbCommand.Parameters.AddWithValue(pair.Key, pair.Value);
                }

                _dbCommand.Prepare();
            }


            using (MySqlDataReader reader = _dbCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return reader;
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
            
            _SetConnection();            

            _dbCommand.CommandText = cmdText;
            _dbCommand.Parameters.Clear();

            if (param.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in param)
                {
                    _dbCommand.Parameters.AddWithValue(pair.Key, pair.Value);
                }

                _dbCommand.Prepare();
            }

            _dbCommand.ExecuteNonQuery();
        }
    }
}
