using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Wplatoon
{
    public static class GearParser
    {
        private static DateTimeOffset ParseUtc(string iso)
        {
            return DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        private static DateTime ToLocal(string iso)
        {
            return ParseUtc(iso).ToLocalTime().DateTime;
        }

        private static string TypeLabel(string typeName)
        {
            switch (typeName)
            {
                case "HeadGear": return "Headgear";
                case "ClothingGear": return "Clothing";
                case "ShoesGear": return "Shoes";
                default: return typeName;
            }
        }
        
        public static List<ItemViewModel> Gear(JObject root)
        {
            var list = new List<ItemViewModel>();

            var pickup = root.SelectToken("data.gesotown.pickupBrand");
            if (pickup != null && pickup.Type != JTokenType.Null)
            {
                string brand = (string)pickup.SelectToken("brand.name");
                var sales = pickup["brandGears"] as JArray;
                if (sales != null)
                    foreach (JObject sale in sales)
                    {
                        var item = FromSale(sale, "Pickup brand: " + brand);
                        if (item != null) list.Add(item);
                    }
            }

            var limited = root.SelectToken("data.gesotown.limitedGears") as JArray;
            if (limited != null)
                foreach (JObject sale in limited)
                {
                    var item = FromSale(sale, "Shop");
                    if (item != null) list.Add(item);
                }

            return list;
        }

        private static ItemViewModel FromSale(JObject sale, string tag)
        {
            var gear = sale["gear"];
            if (gear == null || gear.Type == JTokenType.Null) return null;

            string endIso = (string)sale["saleEndTime"];
            if (endIso != null && ParseUtc(endIso) <= DateTimeOffset.UtcNow) return null; // sale is over

            string fmt = AppSettings.TimeFormat;
            string name = (string)gear["name"];
            string brand = (string)gear.SelectToken("brand.name");
            string usual = (string)gear.SelectToken("brand.usualGearPower.name");
            string main = (string)gear.SelectToken("primaryGearPower.name");
            var subs = gear["additionalGearPowers"] as JArray;
            int slots = subs == null ? 0 : subs.Count;
            string price = ((int)sale["price"]).ToString("N0") + " coins";
            string until = endIso == null ? "" : "Until " + ToLocal(endIso).ToString("ddd " + fmt);
            DateTime? remindAt = null;
            if (endIso != null)
            {
                DateTime r = ToLocal(endIso).AddMinutes(-30);
                if (r > DateTime.Now) remindAt = r;
            }

            return new ItemViewModel
            {
                Title = name,
                Description = brand + " - " + main,
                Subtitle = price + (until.Length > 0 ? ", " + until : ""),
                ImagePath = (string)gear.SelectToken("image.url"),
                Stage1Name = TypeLabel((string)gear["__typename"]),
                Stage2Name = "",
                Stage2Visibility = Visibility.Collapsed,
                TimeRange = until,
                
                Extra = tag
                    + "\nBrand: " + brand + (usual != null ? " (usual ability: " + usual + ")" : "")
                    + "\nMain ability: " + main
                    + "\nSub ability slots: " + slots
                    + "\nPrice: " + price
            };
        }
        
        public static ItemViewModel SalmonReward(JObject coopRoot)
        {
            var g = coopRoot.SelectToken("data.coopResult.monthlyGear");
            if (g == null || g.Type == JTokenType.Null) return null;

            string name = (string)g["name"];
            string type = TypeLabel((string)g["__typename"]);

            return new ItemViewModel
            {
                Title = "Monthly reward gear",
                Description = name,
                Subtitle = "Salmon Run - " + type,
                ImagePath = (string)g.SelectToken("image.url"),
                Stage1Name = name,
                Stage2Name = "",
                Stage2Visibility = Visibility.Collapsed,
                TimeRange = "This month",
                Extra = "Salmon Run reward gear for this month: " + name + " (" + type + ")."
            };
        }
        
        public static List<ItemViewModel> Splatfests(JObject root, string region, int max)
        {
            var list = new List<ItemViewModel>();
            var nodes = root.SelectToken(region + ".data.festRecords.nodes") as JArray;
            if (nodes == null) return list;

            string fmt = AppSettings.TimeFormat;

            foreach (JObject fest in nodes)
            {
                if (list.Count >= max) break;

                DateTime start = ToLocal((string)fest["startTime"]);
                DateTime end = ToLocal((string)fest["endTime"]);
                string title = (string)fest["title"];
                string state = (string)fest["state"];

                var names = new List<string>();
                var lines = new List<string>();
                string winner = null;

                var teams = fest["teams"] as JArray;
                if (teams != null)
                    foreach (JObject t in teams)
                    {
                        string n = (string)t["teamName"];
                        names.Add(n);

                        var res = t["result"];
                        if (res == null || res.Type == JTokenType.Null) continue;

                        if ((bool?)res["isWinner"] == true) winner = n;

                        string line = n;
                        double? vote = (double?)res["voteRatio"];
                        int? pts = (int?)res["totalPoint"];
                        if (vote.HasValue) line += ": " + (vote.Value * 100).ToString("0") + "% of votes";
                        if (pts.HasValue) line += (vote.HasValue ? ", " : ": ") + pts.Value + " pts";
                        lines.Add(line);
                    }

                string status = winner != null ? "winner: " + winner
                              : state == "SCHEDULED" ? "upcoming"
                              : state != "CLOSED" ? "live now"
                              : "";

                list.Add(new ItemViewModel
                {
                    Title = title,
                    Description = string.Join(" vs ", names.ToArray()),
                    Subtitle = start.ToString("MMM d, yyyy") + (status.Length > 0 ? " - " + status : ""),
                    ImagePath = (string)fest.SelectToken("image.url"),
                    Stage1Name = "Splatfest",
                    Stage2Name = "",
                    Stage2Visibility = Visibility.Collapsed,
                    TimeRange = start.ToString("ddd MMM d, " + fmt) + " - " + end.ToString("ddd MMM d, " + fmt),
                    Extra = (winner != null ? "Winner: " + winner + "\n" : "") + string.Join("\n", lines.ToArray()),
                    RemindAt = (state == "SCHEDULED" && start.AddMinutes(-15) > DateTime.Now)
                    ? start.AddMinutes(-15) : (DateTime?)null,
                });
            }
            return list;
        }
    }
}