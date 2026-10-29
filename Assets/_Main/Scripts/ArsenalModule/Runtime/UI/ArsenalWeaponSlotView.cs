using System;
using InventoryModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArsenalModule
{
    /// <summary>
    /// A single owned weapon entry that the player selects to modify.
    /// </summary>
    /// <remarks>
    /// Purely presentational. It renders whatever <see cref="ArsenalWeaponDisplayData"/> it is given and
    /// raises <see cref="Selected"/> when the player clicks it.
    /// </remarks>
    public class ArsenalWeaponSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject selectedHighlight;

        private ItemTable _weapon;

        /// <summary>
        /// Raised with this entry's weapon when the player clicks it.
        /// </summary>
        public event Action<ItemTable> Selected;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClicked);
        }

        /// <summary>
        /// Renders the entry from precomputed display data.
        /// </summary>
        /// <param name="data">The weapon's display data.</param>
        public void Render(ArsenalWeaponDisplayData data)
        {
            _weapon = data.Weapon;

            if (iconImage != null)
                iconImage.sprite = data.Icon;

            if (nameText != null)
                nameText.text = data.Title;

            if (selectedHighlight != null)
                selectedHighlight.SetActive(data.IsSelected);
        }

        private void OnButtonClicked() => Selected?.Invoke(_weapon);
    }
}
