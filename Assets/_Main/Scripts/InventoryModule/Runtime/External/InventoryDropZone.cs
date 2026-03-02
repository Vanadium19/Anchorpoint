using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InventoryModule
{
    public class InventoryDropZone : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image highlightImage;
        [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] private Color hoverColor = new Color(1, 0.5f, 0.5f, 0.5f);

        private IDropService _dropService;

        [Inject]
        public void Construct(IDropService dropService)
        {
            _dropService = dropService;
        }

        public bool TryDropItem(ItemTable item)
        {
            if (_dropService == null) 
                return false;

            if (_dropService.TryDropItem(item))
            {
                item.RemoveItselfFromLocation();
                return true;
            }

            return false;
        }

        public void ShowHighlight(bool show)
        {
            if (highlightImage != null)
                highlightImage.color = show ? hoverColor : normalColor;
        }
    }
}
