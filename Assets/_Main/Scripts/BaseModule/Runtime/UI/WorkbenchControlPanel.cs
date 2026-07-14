using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseModule
{
    public class WorkbenchControlPanel : MonoBehaviour
    {
        [Header("Quantity")]
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minus5Button;
        [SerializeField] private Button minus10Button;
        [SerializeField] private Button plus5Button;
        [SerializeField] private Button plus10Button;
        [SerializeField] private Button maxButton;

        [Header("Craft")]
        [SerializeField] private Button craftButton;

        private int _currentQuantity = 1;
        private int _maxQuantity = 1;
        private bool _isUpdating;
        private System.Action<int> _onCraftRequested;

        public int CurrentQuantity => _currentQuantity;
        public event System.Action<int> QuantityChanged;

        public void Initialize(System.Action<int> onCraftRequested)
        {
            _onCraftRequested = onCraftRequested;

            if (quantityInput != null)
                quantityInput.onEndEdit.AddListener(OnInputEndEdit);

            if (minusButton != null)
                minusButton.onClick.AddListener(() => ChangeQuantity(-1));

            if (plusButton != null)
                plusButton.onClick.AddListener(() => ChangeQuantity(1));

            if (minus5Button != null)
                minus5Button.onClick.AddListener(() => ChangeQuantity(-5));

            if (minus10Button != null)
                minus10Button.onClick.AddListener(() => ChangeQuantity(-10));

            if (plus5Button != null)
                plus5Button.onClick.AddListener(() => ChangeQuantity(5));

            if (plus10Button != null)
                plus10Button.onClick.AddListener(() => ChangeQuantity(10));

            if (maxButton != null)
                maxButton.onClick.AddListener(SetMax);

            if (craftButton != null)
                craftButton.onClick.AddListener(OnCraft);

            Refresh();
        }

        public void SetMaxQuantity(int max)
        {
            _maxQuantity = Mathf.Max(0, max);

            if (maxButton != null)
                maxButton.interactable = _maxQuantity > 0;
        }

        public void SetCraftButtonInteractable(bool interactable)
        {
            if (craftButton != null)
                craftButton.interactable = interactable;
        }

        private void OnInputEndEdit(string input)
        {
            if (_isUpdating)
                return;

            _currentQuantity = int.TryParse(input, out var value) ? Mathf.Clamp(value, 1, 9999) : 1;
            Refresh();
        }

        private void ChangeQuantity(int delta)
        {
            _currentQuantity = Mathf.Clamp(_currentQuantity + delta, 1, 9999);
            Refresh();
        }

        private void SetMax()
        {
            _currentQuantity = Mathf.Clamp(_maxQuantity, 1, 9999);
            Refresh();
        }

        private void OnCraft()
        {
            _onCraftRequested?.Invoke(_currentQuantity);
        }

        private void Refresh()
        {
            _isUpdating = true;

            if (quantityInput != null)
                quantityInput.text = _currentQuantity.ToString();

            _isUpdating = false;

            QuantityChanged?.Invoke(_currentQuantity);
        }
    }
}
