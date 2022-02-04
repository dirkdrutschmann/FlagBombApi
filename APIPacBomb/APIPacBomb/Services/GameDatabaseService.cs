using APIPacBomb.Classes;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Services
{
    /// <summary>
    ///   Datenbankservice für Spielinformationen
    /// </summary>
    public class GameDatabaseService : DatabaseService, Interfaces.IGameDatabaseService
    {
        /// <summary>
        ///   Erstellt eine Instanz des Datenbankdienstes
        /// </summary>
        /// <param name="user">DB-Nutzername</param>
        /// <param name="pass">DB-Passwort</param>
        /// <param name="server">DB-Server</param>
        /// <param name="database">DB-Name</param>
        /// <param name="logger">Logger</param>
        public GameDatabaseService(string user, string pass, string server, string database, ILogger logger) : base(user, pass, server, database, logger)
        { }

        /// <summary>
        ///   Schreibt den Datenbankeintrag für ein abgeschlossenes Spiel
        /// </summary>
        /// <param name="pair">Spielpaarung</param>
        public void WriteGame(UserPlayingPair pair)
        {
            if (pair.Status != UserPlayingPair.PlayingStatus.GAME_OVER)
            {
                return;
            }

            try
            {
                string cmdText = "insert into pb_games " +
                                 "(" +
                                 "  pair_id, " +
                                 "  requested_on, " +
                                 "  map_width, " +
                                 "  map_height, " +
                                 "  capture_flag_count, " +
                                 "  bombs_at_start, " +
                                 "  square_factor " +
                                 ") " +
                                 "values " +
                                 "(" +
                                 "  @pair_id, " +
                                 "  @requested_on, " +
                                 "  @map_width, " +
                                 "  @map_height, " +
                                 "  @capture_flag_count, " +
                                 "  @bombs_at_start, " +
                                 "  @square_factor " +
                                 ")";

                MySqlCommand cmd = new MySqlCommand(cmdText);
                cmd.Parameters.AddWithValue("@pair_id", pair.Id.ToString());
                cmd.Parameters.AddWithValue("@requested_on", pair.RequestTime.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@map_width", pair.Map.Width);
                cmd.Parameters.AddWithValue("@map_height", pair.Map.Heigth);
                cmd.Parameters.AddWithValue("@capture_flag_count", pair.Map.CaptureFlagCount);
                cmd.Parameters.AddWithValue("@bombs_at_start", pair.Map.BombsAtStart);
                cmd.Parameters.AddWithValue("@square_factor", pair.Map.SquareFactor);

                _ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PB_GAMES-Insert fehlgeschlagen fuer {0}.", pair.Id.ToString());
            }

        }

        /// <summary>
        ///   Schreibt die Datenbankeinträge für die teilgenommenen Nutzer
        /// </summary>
        /// <param name="pair">Spielpaarung</param>
        public void WriteUsers(UserPlayingPair pair)
        {
            if (pair.Status != UserPlayingPair.PlayingStatus.GAME_OVER)
            {
                return;
            }

            try
            { 
                string cmdText = "insert into pb_players " +
                                 "(" +
                                 "  pair_id, " +
                                 "  uid, " +
                                 "  requested, " +
                                 "  ip_address " +
                                 ")" +
                                 "values " +
                                 "(" +
                                 "  @pair_id, " +
                                 "  @uid, " +
                                 "  @requested, " +
                                 "  @ip_address " +
                                 ")";



                MySqlCommand cmd = new MySqlCommand(cmdText);
                cmd.Parameters.AddWithValue("@pair_id", pair.Id.ToString());
                cmd.Parameters.AddWithValue("@uid", pair.RequestedUser.Id);
                cmd.Parameters.AddWithValue("@requested", 1);
                cmd.Parameters.AddWithValue("@ip_address", pair.RequestedIP);

                _ExecuteNonQuery(cmd);

                cmd = new MySqlCommand(cmdText);
                cmd.Parameters.AddWithValue("@pair_id", pair.Id.ToString());
                cmd.Parameters.AddWithValue("@uid", pair.RequestingUser.Id);
                cmd.Parameters.AddWithValue("@requested", 0);
                cmd.Parameters.AddWithValue("@ip_address", pair.RequestingIP);

                _ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PB_PLAYERS-Insert fehlgeschlagen fuer {0}.", pair.Id.ToString());
            }
        }

        /// <summary>
        ///   Liefert die Spielehistory eines Nutzers
        /// </summary>
        /// <param name="uid">UserId</param>
        public List<Classes.Responses.GameHistoryEntry> GetGameHistory(int uid)
        {
            string cmdText = "select g.requested_on, " +
                             "       group_concat(u.username separator ', ') as opponent  " +
                             "from   `pb_games`   g,  " +
                             "       `pb_players` p,  " +
                             "       `pb_players` p2, " +
                             "       `pb_users`   u  " +
                             "where  g.pair_id = p.pair_id  " +
                             "and    g.pair_id = p2.pair_id " +
                             "and    p.uid = u.id  " +
                             "and    p.uid != @uid " +
                             "and    p2.uid = @uid " +
                             "group  by g.pair_id  " +
                             "order  by 1 desc  " +
                             "limit  10 ";

            MySqlCommand command = new MySqlCommand(cmdText);
            command.Parameters.AddWithValue("@uid", uid);

            List<Classes.Responses.GameHistoryEntry> gameHistory = new List<Classes.Responses.GameHistoryEntry>();

            foreach(IDataRecord record in _ExecuteQuery(command))
            {
                gameHistory.Add(new Classes.Responses.GameHistoryEntry()
                {
                    RequestedOn  = record.GetDateTime(record.GetOrdinal("requested_on")),
                    Opponent = record.GetString(record.GetOrdinal("opponent"))
                });
            }

            return gameHistory;
        }
    }
}
