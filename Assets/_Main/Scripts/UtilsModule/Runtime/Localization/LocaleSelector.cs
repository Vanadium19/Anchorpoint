using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UtilsModule
{
    public static class LocaleSelector
    {
        private const string LanguagePrefsKey = "selected_language";

        public static void Apply(string localeCode)
        {
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

        public static void RestoreSaved()
        {
            var savedCode = PlayerPrefs.GetString(LanguagePrefsKey, string.Empty);

            if (!string.IsNullOrEmpty(savedCode))
                Apply(savedCode);
        }
    }
}
