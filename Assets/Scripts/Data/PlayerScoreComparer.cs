using System.Collections.Generic;
using DataStructure;

namespace Data
{
    /// <summary>Compares leaderboard entries by descending score.</summary>
    class PlayerScoreComparer : IComparer<PlayerInfo>
    {
        /// <summary>Compares two players so higher scores sort first.</summary>
        public int Compare(PlayerInfo a, PlayerInfo b)
        {
            return b.score.CompareTo(a.score);  
        }
    }
}
