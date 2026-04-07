using System.Collections.Generic;

namespace BuildingModule
{
    public interface IBuildingSaveService
    {
        BuildingSnapshot CreateSnapshot(BuildingView building);
        BuildingView RestoreFromSnapshot(BuildingSnapshot snapshot);
        IReadOnlyList<BuildingSnapshot> GetAllSnapshots();
        void ClearAll();
    }
}
