using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wplatoon;

namespace WplatoonAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private const string Url = "https://splatoon3.ink/data/schedules.json";
        private const string CacheName = "schedules3.json";

        protected override void OnInvoke(ScheduledTask task)
        {
            // the tile only knows Splatoon 3 data, so leave it alone in Splatoon 2 mode
            int game;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue("Game", out game) && game == 2)
            {
                NotifyComplete();
                return;
            }

            string cached = CacheHelper.Load(CacheName);
            if (cached != null && CacheHelper.IsFresh(CacheName))
            {
                if (!TryUpdateTile(cached, NotifyComplete)) NotifyComplete();
                return;
            }

            var client = new WebClient();
            client.Headers["User-Agent"] = "Wplatoon/1.0 (Windows Phone 8 fan app; contact: YOUR-EMAIL-OR-SITE)";
            client.DownloadStringCompleted += (s, e) =>
            {
                string json = (e.Error == null) ? e.Result : cached;   // offline: fall back to what we have
                bool ok = json != null && TryUpdateTile(json, NotifyComplete);
                if (ok && e.Error == null) CacheHelper.Save(CacheName, json);
                if (!ok) NotifyComplete();
            };
            client.DownloadStringAsync(new Uri(Url));
        }

        private static bool TryUpdateTile(string json, Action done)
        {
            try { return UpdateTile(json, done); }
            catch (Exception) { return false; }
        }

        private static bool UpdateTile(string json, Action done)
        {
            JObject current = null;

            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.DateParseHandling = DateParseHandling.None;

                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName || (string)reader.Value != "regularSchedules")
                        continue;

                    // skip to the "nodes" array of the first regularSchedules (the Turf War one)
                    while (reader.Read() && !(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "nodes")) { }
                    reader.Read(); // StartArray

                    while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                    {
                        if (reader.TokenType != JsonToken.StartObject) continue;

                        JObject node = JObject.Load(reader);
                        DateTimeOffset end = DateTimeOffset.Parse((string)node["endTime"],
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                        var setting = node["regularMatchSetting"];

                        if (end > DateTimeOffset.UtcNow && setting != null && setting.Type != JTokenType.Null)
                        {
                            current = node;
                            break;
                        }
                    }
                    break; // only the first one
                }
            }

            if (current == null) return false;

            var s = current["regularMatchSetting"];
            var stages = s["vsStages"] as JArray;
            if (stages == null || stages.Count < 2) return false;

            bool h24;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("Use24Hour", out h24)) h24 = true;
            DateTime endLocal = DateTimeOffset.Parse((string)current["endTime"],
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToLocalTime().DateTime;

            TileUpdater.Update((string)s["vsRule"]["name"],
                               (string)stages[0]["name"], (string)stages[1]["name"],
                               "until " + endLocal.ToString(h24 ? "HH:mm" : "h:mm tt"),
                               (string)stages[0]["image"]["url"], done);
            return true;

            tile.Update(new FlipTileData
            {
                Title = "Wplatoon",
                BackTitle = rule,
                BackContent = s1 + "\n" + s2,
                WideBackContent = s1 + ", " + s2 + "\n" + until
            });
            return true;
        }
    }
}