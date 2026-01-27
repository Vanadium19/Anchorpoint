using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIModule
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text healthText;

        private void Awake()
        {
            if (healthSlider != null)
                healthSlider.interactable = false;
        }

        public void SetHealth(float current, float max)
        {
            if (healthSlider)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText)
                healthText.text = $"{Mathf.CeilToInt(current)} / {max}";
        }
    }
}