using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Interfaces
{
    public interface IGameDatabaseService
    {
        public void WriteGame(Classes.UserPlayingPair pair);

        public void WriteUsers(Classes.UserPlayingPair pair);

        public List<Classes.Responses.GameHistoryEntry> GetGameHistory(int uid);
    }
}
