namespace Systems.Game
{
    public readonly struct RunStats
    {
        public float ElapsedTime { get; }
        public int Kills { get; }
        public int RoomsCleared { get; }

        public RunStats(float elapsedTime, int kills, int roomsCleared)
        {
            ElapsedTime = elapsedTime;
            Kills = kills;
            RoomsCleared = roomsCleared;
        }
    }
}
