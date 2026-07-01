namespace SpawnModule
{
    public sealed class SpawnConfig
    {
        public SpawnConfig(
            int initialEnemyCount,
            int enemiesPerWave,
            int firstWaveDelay,
            int waveInterval)
        {
            InitialEnemyCount = initialEnemyCount;
            EnemiesPerWave = enemiesPerWave;
            FirstWaveDelay = firstWaveDelay;
            WaveInterval = waveInterval;
        }

        public int InitialEnemyCount { get; }
        public int EnemiesPerWave { get; }
        public int FirstWaveDelay { get; }
        public int WaveInterval { get; }
    }
}
