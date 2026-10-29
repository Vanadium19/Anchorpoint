using InventoryModule;
using UnityEngine;
using UtilsModule;

namespace ArsenalModule
{
    /// <summary>
    /// World component that marks a building as an Arsenal and exposes its price configuration.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IExternalUI"/> so <c>PlayerModule.PlayerInteractionController</c> opens the
    /// assigned <see cref="UIPrefab"/> through <c>InventoryModule.ExternalUIManager</c> without any change
    /// to player interaction code. It holds no installation logic; that lives in
    /// <see cref="ArsenalService"/> and <see cref="ArsenalPresenter"/>.
    /// </remarks>
    public class ArsenalStationView : MonoBehaviour, IExternalUI, IArsenalStation
    {
        [SerializeField] private string displayName = "Arsenal";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private ArsenalConfig config;

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        /// <inheritdoc/>
        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);

        /// <inheritdoc/>
        public GameObject UIPrefab => uiPrefab;

        /// <inheritdoc/>
        public ArsenalConfig Config => config;
    }
}
