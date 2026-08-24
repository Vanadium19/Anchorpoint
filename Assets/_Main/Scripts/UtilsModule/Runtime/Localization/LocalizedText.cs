using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace UtilsModule
{
    public static class LocalizedText
    {
        private const string UiTableName = "Localization";

        public static string Get(string key)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(UiTableName, key);
        }

        public static string GetFormatted(string key, params object[] args)
        {
            var format = Get(key);

            return args.Length > 0 ? string.Format(format, args) : format;
        }
    }
}
