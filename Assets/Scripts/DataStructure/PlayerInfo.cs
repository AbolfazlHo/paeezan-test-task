namespace DataStructure
{
    /// <summary>Stores a leaderboard player's displayed identity, rank, score, and currency.</summary>
    [System.Serializable]
    public class PlayerInfo
    {
        /// <summary>Displayed player name.</summary>
        public string name;
        /// <summary>Leaderboard position.</summary>
        public int rank;
        /// <summary>Player's recorded score.</summary>
        public int score;
        /// <summary>Whether this entry represents the local player.</summary>
        public bool isUser;
        /// <summary>Player's saved currency balance.</summary>
        public int currency;
    }
}
