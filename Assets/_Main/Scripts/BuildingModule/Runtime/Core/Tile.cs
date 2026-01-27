using UnityEngine;

namespace BuildingModule
{
    public class Tile
    {
        private readonly Vector3 _worldPosition;

        private bool _isOccupied;

        public Tile(Vector3 worldPosition)
        {
            _worldPosition = worldPosition;
        }

        public Vector3 WorldPosition => _worldPosition;
        public bool IsOccupied => _isOccupied;

        public void Occupy() => _isOccupied = true;
    }
}