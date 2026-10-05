using System;

namespace BuildingModule
{
    public sealed class MinefieldModel
    {
        private int _remainingMines;

        public event Action<int> MinesChanged;

        public MinefieldModel(int mineCount)
        {
            _remainingMines = Math.Max(mineCount, 1);
        }

        public int RemainingMines => _remainingMines;
        public bool HasMines => _remainingMines > 0;

        public bool TryConsumeMine()
        {
            if (_remainingMines <= 0)
                return false;

            _remainingMines--;
            MinesChanged?.Invoke(_remainingMines);

            return true;
        }
    }
}
