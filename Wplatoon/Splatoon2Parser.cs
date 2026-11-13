using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Wplatoon
{
    public static class Splatoon2Parser
    {
        private const string ImageBase = "https://splatoon2.ink/assets/splatnet";
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long NowUnix() { return (long)(DateTime.UtcNow - Epoch).TotalSeconds; }
        private static DateTime ToLocal(long unix) { return Epoch.AddSeconds(unix).ToLocalTime(); }

        private static string Img(JToken path)
        {
            string p = path == null ? null : (string)path;
            if (string.IsNullOrEmpty(p)) return null;
            return p.StartsWith("http") ? p : ImageBase + p;
        }

        private static ItemViewModel FromBattle(JObject node)
        {
            var a = node["stage_a"];
            var b = node["stage_b"];
            if (a == null || b == null) return null;

            string fmt = AppSettings.TimeFormat;
            DateTime start = ToLocal((long)node["start_time"]);
            DateTime end = ToLocal((long)node["end_time"]);
            string s1 = (string)a["name"];
            string s2 = (string)b["name"];

            return new ItemViewModel
            {
                Title = (string)node["rule"]["name"],
                Description = s1 + ", " + s2,
                Subtitle = start.ToString(fmt) + " - " + end.ToString(fmt),
                ImagePath = Img(a["image"]),
                Stage1Name = s1,
                Stage2Name = s2,
                Image2Path = Img(b["image"]),
                TimeRange = start.ToString("ddd " + fmt) + " - " + end.ToString(fmt)
            };
        }

        private static List<ItemViewModel> Battles(JObject root, string key)
        {
            var list = new List<ItemViewModel>();
            var nodes = root[key] as JArray;
            if (nodes == null) return list;

            long now = NowUnix();
            foreach (JObject node in nodes)
            {
                if ((long)node["end_time"] <= now) continue; // already over
                var item = FromBattle(node);
                if (item != null) list.Add(item);
            }
            return list;
        }

        public static List<ItemViewModel> TurfWar(JObject root) { return Battles(root, "regular"); }
        public static List<ItemViewModel> Ranked(JObject root) { return Battles(root, "gachi"); }
        public static List<ItemViewModel> League(JObject root) { return Battles(root, "league"); }

        public static DateTimeOffset? CurrentRotationEnd(JObject root)
        {
            var nodes = root["regular"] as JArray;
            if (nodes == null) return null;

            long now = NowUnix();
            foreach (JObject node in nodes)
            {
                long end = (long)node["end_time"];
                if (end > now) return new DateTimeOffset(Epoch.AddSeconds(end));
            }
            return null;
        }

        private static string WeaponName(JToken w)
        {
            var t = w["weapon"] ?? w["coop_special_weapon"];
            return t != null ? (string)t["name"] : "?";
        }

        // coop-schedules.json
        public static List<ItemViewModel> SalmonRun(JObject coopRoot)
        {
            var list = new List<ItemViewModel>();
            var details = coopRoot["details"] as JArray;
            if (details == null) return list;

            long now = NowUnix();
            string fmt = AppSettings.TimeFormat;

            foreach (JObject d in details)
            {
                if ((long)d["end_time"] <= now) continue;

                DateTime start = ToLocal((long)d["start_time"]);
                DateTime end = ToLocal((long)d["end_time"]);
                string range = start.ToString("ddd " + fmt) + " - " + end.ToString("ddd " + fmt);

                string stage = (string)d["stage"]["name"];
                var ws = d["weapons"] as JArray ?? new JArray();
                string weapons = string.Join(", ", ws.Select(w => WeaponName(w)).ToArray());

                list.Add(new ItemViewModel
                {
                    Title = "Salmon Run",
                    Description = stage + ": " + weapons,
                    Subtitle = range,
                    ImagePath = Img(d["stage"]["image"]),
                    Stage1Name = stage,
                    Stage2Name = "",
                    Stage2Visibility = Visibility.Collapsed,
                    TimeRange = range,
                    Extra = "Weapons: " + weapons
                });
            }
            return list;
        }
    }
}