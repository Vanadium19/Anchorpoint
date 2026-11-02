using TMPro;
using UnityEngine;

namespace ArsenalModule
{
    /// <summary>
    /// One stat line: label, current value and, when a modification is previewed, the value after installing it.
    /// </summary>
    public class ArsenalStatRowView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI currentValueText;
        [SerializeField] private GameObject previewRoot;
        [SerializeField] private TextMeshProUGUI previewValueText;
        [SerializeField] private Color improvementColor = Color.green;
        [SerializeField] private Color worseningColor = Color.red;

        /// <summary>
        /// Renders the stat line from precomputed display data.
        /// </summary>
        /// <param name="data">The stat's display data, including the preview value.</param>
        public void Render(ArsenalStatDisplayData data)
        {
            if (labelText != null)
                labelText.text = data.LabelText;

            if (currentValueText != null)
                currentValueText.text = data.CurrentValueText;

            if (previewRoot != null)
                previewRoot.SetActive(data.HasPreview);

            if (previewValueText != null)
            {
                previewValueText.text = data.PreviewValueText;
                previewValueText.color = data.IsImprovement ? improvementColor : worseningColor;
            }
        }
    }
}
