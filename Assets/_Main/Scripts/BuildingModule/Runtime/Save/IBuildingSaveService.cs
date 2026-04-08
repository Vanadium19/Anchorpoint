using System.Collections.Generic;

namespace BuildingModule
{
    public interface IBuildingSaveService
    {
        BuildingView RestoreFromSnapshot(BuildingSnapshot snapshot);
        IReadOnlyList<BuildingSnapshot> GetAllSnapshots();
        void ClearAll();
    }
}
