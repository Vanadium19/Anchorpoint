using System;

namespace BuildingModule
{
    public class BuildingUpgradeHoldModel
    {
        private const float HoldSeconds = 1.5f;

        private float _elapsedSeconds;
        private bool _mustRelease;

        public float Progress => _elapsedSeconds / HoldSeconds;

        public void Reset(bool isHeld)
        {
            _elapsedSeconds = 0f;
            _mustRelease = isHeld;
        }

        public bool Advance(bool isHeld, bool canAfford, float deltaSeconds)
        {
            if (!isHeld)
            {
                Reset(false);
                return false;
            }

            if (!canAfford)
            {
                Reset(true);
                return false;
            }

            if (_mustRelease)
                return false;

            _elapsedSeconds = Math.Min(HoldSeconds, _elapsedSeconds + Math.Max(0f, deltaSeconds));

            if (_elapsedSeconds < HoldSeconds)
                return false;

            _mustRelease = true;
            return true;
        }
    }
}
