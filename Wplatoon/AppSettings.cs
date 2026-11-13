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

        public const string UserAgent = "Wplatoon/1.0 (Windows Phone 8 fan app; contact: sousand@proton.me)";

        // 3 = Splatoon 3 (splatoon3.ink), 2 = Splatoon 2 (splatoon2.ink)
        public static int Game
        {
            get { return Get("Game", 3); }
            set { Set("Game", value); }
        }

        public static string GameName
        {
            get { return Game == 2 ? "Splatoon 2" : "Splatoon 3"; }
        }

        public static string SchedulesUrl
        {
            get
            {
                return Game == 2 ? "https://splatoon2.ink/data/schedules.json"
                                 : "https://splatoon3.ink/data/schedules.json";
            }
        }

        public static string SchedulesCache
        {
            get { return Game == 2 ? "schedules2.json" : "schedules3.json"; }
        }

        public const string Gear3Url = "https://splatoon3.ink/data/gear.json";
        public const string Gear3Cache = "gear3.json";
        public const string Coop3Url = "https://splatoon3.ink/data/coop.json";
        public const string Coop3Cache = "coop3.json";
        public const string Fest3Url = "https://splatoon3.ink/data/festivals.json";
        public const string Fest3Cache = "festivals3.json";
        public const string Coop2Url = "https://splatoon2.ink/data/coop-schedules.json";
        public const string Coop2Cache = "coop2.json";

        public static string RegionCode
        {
            get
            {
                int i = RegionIndex;
                return RegionCodes[(i >= 0 && i < RegionCodes.Length) ? i : 0];
            }
        }
    }
}