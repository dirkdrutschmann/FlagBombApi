using APIPacBomb.Classes;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    /// <summary>
    ///   Datenbank-Service
    /// </summary>
    public class DatabaseService
    {
        /// <summary>
        ///   Logger-Instanz
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        ///   Steuervariable für Thread
        /// </summary>
        private bool _stopThread = false;

        /// <summary>
        ///   Thread für Queueabarbeitung
        /// </summary>
        private Thread _queueThread;

        /// <summary>
        ///   Abarbeitungsreihe von Befehlen
        /// </summary>
        private Queue<MySqlCommand> _commandQueue;

        /// <summary>
        ///   Datenbankverbindung für DML
        /// </summary>
        private MySqlConnection _dbExecuteConnection;

        /// <summary>
        ///   Datenbankverbindung für SQL
        /// </summary>
        private MySqlConnection _dbSelectConnection;

        /// <summary>
        ///   Verbindungszeichenkette
        /// </summary>
        private readonly string _ConnectionString;     

        /// <summary>
        ///   Erstellt eine Instanz des Datenbankdienstes
        /// </summary>
        /// <param name="user">DB-Nutzername</param>
        /// <param name="pass">DB-Passwort</param>
        /// <param name="server">DB-Server</param>
        /// <param name="database">DB-Name</param>
        /// <param name="logger">Logger</param>
        public DatabaseService(string user, string pass, string server, string database, ILogger logger)
        {
            _logger = logger;

            _ConnectionString = string.Format(
                "SERVER={0};DATABASE={1};UID={2};PASSWORD={3};SSL Mode=None;convert zero datetime=True",
                server,
                database,
                user,
                pass
            );

            _dbExecuteConnection = new MySqlConnection(_ConnectionString);
            _dbSelectConnection = new MySqlConnection(_ConnectionString);

            _commandQueue = new Queue<MySqlCommand>();
            _StartQueueThread();
        }

        /// <summary>
        ///   Destruktor
        /// </summary>
        ~DatabaseService()
        {
            _stopThread = true;
            
            _dbExecuteConnection.Dispose();
            _dbExecuteConnection = null;

            _dbSelectConnection.Dispose();
            _dbSelectConnection = null;
        }

        /// <summary>
        ///   Liefert die geöffnete Datenbankverbindung zurück
        /// </summary>
        /// <returns>Datenbankverbindung</returns>
        private MySqlConnection _GetDMLConnection()
        {
            if (_dbExecuteConnection.State == ConnectionState.Closed || _dbExecuteConnection.State == ConnectionState.Broken)
            {
                _dbExecuteConnection.Dispose();

                _dbExecuteConnection = new MySqlConnection(_ConnectionString);
                _dbExecuteConnection.Open();
            }

            return _dbExecuteConnection;
        }

        /// <summary>
        ///   Liefert die geöffnete Datenbankverbindung zurück
        /// </summary>
        /// <returns>Datenbankverbindung</returns>
        private MySqlConnection _GetSQLConnection()
        {
            if (_dbSelectConnection.State == ConnectionState.Closed || _dbSelectConnection.State == ConnectionState.Broken)
            {
                _dbSelectConnection.Dispose();

                _dbSelectConnection = new MySqlConnection(_ConnectionString);
                _dbSelectConnection.Open();
            }

            return _dbSelectConnection;
        }

        /// <summary>
        ///   Liefert das Result-Set als abzählbare Recordsammlung
        /// </summary>
        /// <param name="query">Abfrage</param>
        /// <param name="param">Abfrageparameter</param>
        /// <returns>Result-Set</returns>
        protected IEnumerable<IDataRecord> _ExecuteQuery(string query, List<KeyValuePair<string, string>> param)
        {
            using (MySqlCommand cmd = new MySqlCommand
            {
                Connection = _GetSQLConnection(),
                CommandText = query
            })
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
                    _logger.LogInformation("Executed select: {0}", cmd.CommandText);

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
            MySqlCommand cmd = new MySqlCommand();
            
            cmd.CommandText = cmdText;            

            if (param.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in param)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            }

            _ExecuteNonQuery(cmd);
        }

        protected void _ExecuteNonQuery(MySqlCommand cmd)
        {
            _commandQueue.Enqueue(cmd);
        }

        /// <summary>
        ///   Implementiert und Startet den Queue-Thread
        /// </summary>
        private void _StartQueueThread()
        {
            if (_queueThread != null)
            {
                return;
            }

            _queueThread = new Thread(new ThreadStart(
                () =>
                {
                    while (!_stopThread)
                    {
                        if (_commandQueue.Count == 0)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        MySqlCommand command = _commandQueue.Dequeue();
                        command.Connection = _GetDMLConnection();

                        MySqlTransaction transaction = command.Connection.BeginTransaction();

                        if (command.Parameters.Count > 0 && !command.IsPrepared)
                        {
                            command.Prepare();
                        }

                        command.ExecuteNonQuery();
                        transaction.Commit();

                        _logger.LogInformation("Executed command: {0}", command.CommandText);
                    }
                })
            );

            _queueThread.Start();
        }
    }
}
