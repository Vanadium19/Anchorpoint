using UnityEngine;
using UnityEngine.UI;

namespace BuildingModule
{
    public class BuildingMenuItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionImage;
        [SerializeField] private CanvasGroup canvasGroup;

        public RectTransform RectTransform => (RectTransform)transform;

        public void SetData(BuildingMenuItem item)
        {
            iconImage.sprite = item.Icon;
        }

        public void SetSelected(bool selected)
        {
            selectionImage.gameObject.SetActive(selected);
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
