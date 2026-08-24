using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using UtilsModule;

namespace BaseModule
{
    public class Workbench : MonoBehaviour, IExternalUI, IRecipeProvider, IHasInstanceId
    {
        [SerializeField] private string displayName = "Workbench";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private List<RecipeConfig> recipes = new();
        [SerializeField] private string _instanceId;

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<RecipeConfig> Recipes => recipes;
        public string InstanceId { get => _instanceId; set => _instanceId = value; }

        private void Awake()
        {
            if (string.IsNullOrEmpty(_instanceId))
                _instanceId = Guid.NewGuid().ToString();
        }
    }
}
