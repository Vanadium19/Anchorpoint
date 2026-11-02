using InventoryModule;
using UnityEngine;
using UtilsModule;

namespace ArsenalModule
{
    /// <summary>
    /// World component that marks a building as the Arsenal, where weapons are modified.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IExternalUI"/> so <c>PlayerModule.PlayerInteractionController</c> opens
    /// the assigned <see cref="UIPrefab"/> through <c>InventoryModule.ExternalUIManager</c>. It holds no
    /// logic; that lives in <see cref="ArsenalService"/> and <see cref="ArsenalPresenter"/>.
    /// </remarks>
    public class ArsenalView : MonoBehaviour, IExternalUI
    {
        [SerializeField] private string displayName = "Arsenal";
        [SerializeField] private string nameKey = "";
        [SerializeField] private GameObject uiPrefab;

        /// <inheritdoc/>
        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);

        /// <inheritdoc/>
        public GameObject UIPrefab => uiPrefab;
    }
}
