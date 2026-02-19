using System.Collections.Generic;
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

        public int PocketCount => grids != null ? grids.Length : 0;

        public void InitializeGrids()
        {
            if (grids == null || grids.Length == 0)
            {
                grids = GetComponentsInChildren<AbstractGrid>();
            }
        }

        public AbstractGrid GetGrid(int index)
        {
            if (grids == null || index < 0 || index >= grids.Length)
                return null;
            return grids[index];
        }

        public AbstractGrid[] GetGridsFromPanel(GameObject panelInstance)
        {
            if (panelInstance == null)
                return null;

            return panelInstance.GetComponentsInChildren<AbstractGrid>();
        }
    }
}
