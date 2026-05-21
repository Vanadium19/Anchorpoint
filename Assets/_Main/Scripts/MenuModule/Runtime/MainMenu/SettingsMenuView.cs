using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenuModule
{
    public class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;

        public bool IsVisible => Target.activeSelf;

        private GameObject Target => panel != null ? panel : gameObject;

        private void OnEnable()
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        private void OnDisable()
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }

        public void Show() => Target.SetActive(true);

        public void Hide() => Target.SetActive(false);

        public void SetVisible(bool isVisible) => Target.SetActive(isVisible);

        public void Toggle() => SetVisible(!IsVisible);

        public void SetMusicVolume(float value)
        {
            if (musicVolumeSlider != null)
                musicVolumeSlider.SetValueWithoutNotify(value);
        }

        public void SetSfxVolume(float value)
        {
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.SetValueWithoutNotify(value);
        }

        private void OnMusicVolumeChanged(float value) => MusicVolumeChanged?.Invoke(value);

        private void OnSfxVolumeChanged(float value) => SfxVolumeChanged?.Invoke(value);
    }
}
