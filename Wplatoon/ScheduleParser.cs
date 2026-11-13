using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Wplatoon
{
    public static class ScheduleParser
    {
        private static JArray Nodes(JObject root, string path)
        {
            return root.SelectToken(path) as JArray ?? new JArray();
        }

        private static DateTimeOffset ParseUtc(string iso)
        {
            return DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        private static DateTime ToLocal(string iso)
        {
            return ParseUtc(iso).ToLocalTime().DateTime;
        }

        private static bool HasEnded(JObject node)
        {
            return ParseUtc((string)node["endTime"]) <= DateTimeOffset.UtcNow;
        }

        public static DateTimeOffset? CurrentRotationEnd(JObject root)
        {
            foreach (JObject node in Nodes(root, "data.regularSchedules.nodes"))
                if (!HasEnded(node)) return ParseUtc((string)node["endTime"]);
            return null;
        }
        
        private static ItemViewModel FromVsSetting(JObject node, JToken setting, string prefix)
        {
            if (setting == null || setting.Type == JTokenType.Null) return null;
            var stages = setting["vsStages"] as JArray;
            if (stages == null || stages.Count < 2) return null;

            string fmt = AppSettings.TimeFormat;
            DateTime start = ToLocal((string)node["startTime"]);
            DateTime end = ToLocal((string)node["endTime"]);
            string s1 = (string)stages[0]["name"];
            string s2 = (string)stages[1]["name"];

            return new ItemViewModel
            {
                Title = prefix + (string)setting["vsRule"]["name"],
                Description = s1 + ", " + s2,
                Subtitle = start.ToString(fmt) + " - " + end.ToString(fmt),
                ImagePath = (string)stages[0]["image"]["url"],
                Stage1Name = s1,
                Stage2Name = s2,
                Image2Path = (string)stages[1]["image"]["url"],
                TimeRange = start.ToString("ddd " + fmt) + " - " + end.ToString(fmt)
            };
        }

        public static List<ItemViewModel> TurfWar(JObject root)
        {
            var list = new List<ItemViewModel>();
            foreach (JObject node in Nodes(root, "data.regularSchedules.nodes"))
            {
                if (HasEnded(node)) continue;
                var item = FromVsSetting(node, node["regularMatchSetting"], "");
                if (item != null) list.Add(item); // null during a Splatfest
            }
            return list;
        }

        public static List<ItemViewModel> Anarchy(JObject root)
        {
            var list = new List<ItemViewModel>();
            foreach (JObject node in Nodes(root, "data.bankaraSchedules.nodes"))
            {
                if (HasEnded(node)) continue;
                var settings = node["bankaraMatchSettings"] as JArray;
                if (settings == null) continue;

                foreach (JObject s in settings)
                {
                    string mode = (string)s["bankaraMode"] == "CHALLENGE" ? "Series: " : "Open: ";
                    var item = FromVsSetting(node, s, mode);
                    if (item != null) list.Add(item);
                }
            }
            return list;
        }

        public static List<ItemViewModel> XBattles(JObject root)
        {
            var list = new List<ItemViewModel>();
            foreach (JObject node in Nodes(root, "data.xSchedules.nodes"))
            {
                if (HasEnded(node)) continue;
                var item = FromVsSetting(node, node["xMatchSetting"], "");
                if (item != null) list.Add(item);
            }
            return list;
        }

        public static List<ItemViewModel> SalmonRun(JObject root)
        {
            var list = new List<ItemViewModel>();
            AddCoop(list, root, "data.coopGroupingSchedule.regularSchedules.nodes", "Salmon Run");
            AddCoop(list, root, "data.coopGroupingSchedule.bigRunSchedules.nodes", "Big Run");
            AddCoop(list, root, "data.coopGroupingSchedule.teamContestSchedules.nodes", "Eggstra Work");
            return list;
        }

        private static void AddCoop(List<ItemViewModel> list, JObject root, string path, string title)
        {
            foreach (JObject node in Nodes(root, path))
            {
                if (HasEnded(node)) continue;
                var setting = node["setting"];
                if (setting == null || setting.Type == JTokenType.Null) continue;

                string fmt = AppSettings.TimeFormat;
                DateTime start = ToLocal((string)node["startTime"]);
                DateTime end = ToLocal((string)node["endTime"]);
                string range = start.ToString("ddd " + fmt) + " - " + end.ToString("ddd " + fmt);

                string stage = (string)setting["coopStage"]["name"];
                var weaponArray = setting["weapons"] as JArray ?? new JArray();
                string weapons = string.Join(", ", weaponArray.Select(w => (string)w["name"]).ToArray());

                var boss = setting["boss"];
                string king = (boss != null && boss.Type != JTokenType.Null) ? (string)boss["name"] : null;

                list.Add(new ItemViewModel
                {
                    Title = title,
                    Description = stage + ": " + weapons,
                    Subtitle = range,
                    ImagePath = (string)setting["coopStage"]["thumbnailImage"]["url"],
                    Stage1Name = stage,
                    Stage2Name = "",
                    Image2Path = null,
                    Stage2Visibility = Visibility.Collapsed,
                    TimeRange = range,
                    Extra = (king != null ? "King Salmonid: " + king + "\n" : "") + "Weapons: " + weapons
                });
            }
        }
    }
}