using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UtilsModule
{
    public static class LocaleSelector
    {
        private const string LanguagePrefsKey = "selected_language";

        public static async UniTask ApplyAsync(string localeCode, CancellationToken token)
        {
            if (string.IsNullOrEmpty(localeCode))
                return;

            var canceled = await UniTask
                .WaitUntil(IsLocalizationReady, cancellationToken: token)
                .SuppressCancellationThrow();

            if (canceled || !IsLocalizationReady())
                return;

            var availableLocales = LocalizationSettings.AvailableLocales;

            if (availableLocales == null)
                return;

            var locale = availableLocales.GetLocale(localeCode);

            if (locale == null)
                return;

            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LanguagePrefsKey, localeCode);
            PlayerPrefs.Save();
        }

        public static async UniTask RestoreSavedAsync(CancellationToken token)
        {
            var savedCode = PlayerPrefs.GetString(LanguagePrefsKey, string.Empty);

            if (!string.IsNullOrEmpty(savedCode))
                await ApplyAsync(savedCode, token);
        }

        private static bool IsLocalizationReady() =>
            LocalizationSettings.InitializationOperation.IsDone;
    }
}
