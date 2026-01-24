using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayerModule.View
{
    public class PlayerHUDView : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text healthText;
        public void SetHealth(float current, float max)
        {
            if (healthSlider)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
            if (healthText)
            {
                healthText.text = $"{Mathf.CeilToInt(current)} / {max}";
            }
        }
    }
}