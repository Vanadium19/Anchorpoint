using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AudioModule
{
    public class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [SerializeField] private Button[] openButtons;
        [SerializeField] private Button[] closeButtons;

        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;

        public bool IsVisible => _target.activeSelf;

        private GameObject _target => panel ? panel : gameObject;

        private void OnEnable()
        {
            AddListeners(openButtons, OnOpenClicked);
            AddListeners(closeButtons, OnCloseClicked);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        private void OnDisable()
        {
            RemoveListeners(openButtons, OnOpenClicked);
            RemoveListeners(closeButtons, OnCloseClicked);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }

        public void Show() => _target?.SetActive(true);

        public void Hide() => _target?.SetActive(false);

        public void SetVisible(bool isVisible) => _target.SetActive(isVisible);

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

        private void OnOpenClicked() => Show();

        private void OnCloseClicked() => Hide();

        private void AddListeners(Button[] buttons, UnityAction action)
        {
            foreach (var button in buttons)
                button.onClick.AddListener(action);
        }

        private void RemoveListeners(Button[] buttons, UnityAction action)
        {
            foreach (var button in buttons)
                button.onClick.RemoveListener(action);
        }
    }
}