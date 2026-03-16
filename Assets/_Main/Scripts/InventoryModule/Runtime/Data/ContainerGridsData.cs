using UnityEngine;

namespace InventoryModule
{
    public class ContainerGridsData : MonoBehaviour
    {
        [Header("Container Panel Prefab")]
        [SerializeField] private GameObject containerPanelPrefab;

        [Header("Individual Grids (if no panel prefab)")]
        [SerializeField] private AbstractGrid[] grids;

        public GameObject ContainerPanelPrefab => containerPanelPrefab;
        public AbstractGrid[] Grids => grids;

        public void InitializeGrids()
        {
            if (grids == null || grids.Length == 0)
                grids = GetComponentsInChildren<AbstractGrid>();
        }

        public AbstractGrid[] GetGridsFromPanel(GameObject panelInstance)
            => panelInstance == null ? null : panelInstance.GetComponentsInChildren<AbstractGrid>();
    }
}