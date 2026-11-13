using System.IO.IsolatedStorage;

namespace Wplatoon
{
    public static class AppSettings
    {
        private static readonly IsolatedStorageSettings S = IsolatedStorageSettings.ApplicationSettings;

        public static readonly string[] RegionNames =
        {
            "Americas / Australia / NZ", "Europe", "Japan", "Hong Kong / South Korea"
        };
        public static readonly string[] RegionCodes = { "US", "EU", "JP", "AP" };

        public static readonly string[] LanguageNames =
        {
            "English (US)", "English (GB)", "Español (ES)", "Español (MX)",
            "Français (FR)", "Deutsch", "Italiano", "日本語"
        };
        public static readonly string[] LanguageCodes =
        {
            "en-US", "en-GB", "es-ES", "es-MX", "fr-FR", "de-DE", "it-IT", "ja-JP"
        };

        private static T Get<T>(string key, T defaultValue)
        {
            T value;
            return S.TryGetValue(key, out value) ? value : defaultValue;
        }

        private static void Set(string key, object value)
        {
            S[key] = value;
            S.Save();
        }

        public static bool Use24Hour
        {
            get { return Get("Use24Hour", true); }
            set { Set("Use24Hour", value); }
        }

        public static int RegionIndex
        {
            get { return Get("RegionIndex", 0); }
            set { Set("RegionIndex", value); }
        }

        public static int LanguageIndex
        {
            get { return Get("LanguageIndex", 0); }
            set { Set("LanguageIndex", value); }
        }

        public static string TimeFormat
        {
            get { return Use24Hour ? "HH:mm" : "h:mm tt"; }
        }
    }
}