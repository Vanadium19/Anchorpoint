using System;
using InventoryModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArsenalModule
{
    /// <summary>
    /// A single weapon row: the weapon name and a button to select it.
    /// </summary>
    /// <remarks>
    /// Purely presentational. It renders whatever <see cref="ArsenalWeaponDisplayData"/> it is given and
    /// raises <see cref="Selected"/> when the player clicks the row.
    /// </remarks>
    public class ArsenalWeaponRowView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedIndicator;

        private ItemTable _weapon;

        /// <summary>
        /// Raised with this row's weapon when the player clicks the row.
        /// </summary>
        public event Action<ItemTable> Selected;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClicked);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(OnSelectClicked);
        }

        /// <summary>
        /// Renders the row from precomputed display data.
        /// </summary>
        /// <param name="data">The weapon's display data.</param>
        public void Render(ArsenalWeaponDisplayData data)
        {
            _weapon = data.Weapon;

            if (nameText != null)
                nameText.text = data.NameText;

            if (selectedIndicator != null)
                selectedIndicator.SetActive(data.IsSelected);
        }

        private void OnSelectClicked() => Selected?.Invoke(_weapon);
    }
}
