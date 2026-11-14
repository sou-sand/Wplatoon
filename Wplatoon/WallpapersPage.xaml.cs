using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;
using Windows.Phone.System.UserProfile;

namespace Wplatoon
{
    public class WallpaperItem
    {
        public string Name { get; set; }
        public string ThumbUrl { get; set; }
        public bool IsLocal { get; set; }
        public string File { get; set; }
        public string HighUrl { get; set; }
    }

    public partial class WallpapersPage : PhoneApplicationPage
    {
        private const string LocalDir = "Assets/defined/wallpapers/";

        public WallpapersPage()
        {
            InitializeComponent();
            LoadLocal();
            LoadServer();
        }

        private void LoadLocal()
        {
            var items = new List<WallpaperItem>();
            try
            {
                var res = Application.GetResourceStream(new Uri(LocalDir + "list.txt", UriKind.Relative));
                if (res != null)
                    using (var reader = new StreamReader(res.Stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.Length == 0 || line.StartsWith("#")) continue;

                            string[] parts = line.Split('|');
                            string file = parts[0].Trim();
                            string name = parts.Length > 1 ? parts[1].Trim()
                                                           : Path.GetFileNameWithoutExtension(file);
                            items.Add(new WallpaperItem
                            {
                                Name = name,
                                IsLocal = true,
                                File = LocalDir + file,
                                ThumbUrl = "/" + LocalDir + file
                            });
                        }
                    }
            }
            catch (Exception) { }

            LocalList.ItemsSource = items;
            LocalEmpty.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        // server (not sure if this will work)
        private void LoadServer()
        {
            string json = CacheHelper.Load("schedules3.json");
            if (json == null)
            {
                Info.Text = "No Splatoon 3 data yet. Open the main page with Splatoon 3 selected once, then come back.";
                return;
            }

            var items = new List<WallpaperItem>();
            var seen = new HashSet<string>();
            var root = JObject.Parse(json);

            var stages = root.SelectToken("data.vsStages.nodes") as JArray;
            if (stages != null)
                foreach (JObject s in stages)
                    AddServer(items, seen, (string)s["name"], (string)s.SelectToken("originalImage.url"));

            var coop = root.SelectToken("data.coopGroupingSchedule.regularSchedules.nodes") as JArray;
            if (coop != null)
                foreach (JObject n in coop)
                    AddServer(items, seen, (string)n.SelectToken("setting.coopStage.name"),
                                           (string)n.SelectToken("setting.coopStage.image.url"));

            ServerList.ItemsSource = items;
        }

        private static void AddServer(List<WallpaperItem> items, HashSet<string> seen, string name, string url)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url) || !seen.Add(url)) return;
            items.Add(new WallpaperItem
            {
                Name = name,
                HighUrl = url,
                ThumbUrl = url.Replace("high_resolution", "low_resolution").Replace("_0.png", "_1.png")
            });
        }

        private async void Wall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            var item = list.SelectedItem as WallpaperItem;
            if (item == null) return;
            list.SelectedItem = null;

            if (MessageBox.Show("Set " + item.Name + " as your lock screen?", "Lock screen",
                                MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            Busy(true);
            try
            {
                string uri;
                string downloaded = null;

                if (item.IsLocal)
                {
                    uri = "ms-appx:///" + item.File;            // straight from the app package
                }
                else
                {
                    downloaded = await Download(item.HighUrl);  // saved in isolated storage first
                    uri = "ms-appdata:///local/" + downloaded;
                }

                if (!LockScreenManager.IsProvidedByCurrentApplication)
                {
                    var access = await LockScreenManager.RequestAccessAsync();
                    if (access != LockScreenRequestResult.Granted)
                    {
                        MessageBox.Show("Wplatoon wasn't allowed to change the lock screen.",
                                        "Lock screen", MessageBoxButton.OK);
                        return;
                    }
                }

                LockScreen.SetImageUri(new Uri(uri, UriKind.Absolute));
                DeleteOldImages(downloaded);
                MessageBox.Show(item.Name + " is now your lock screen.", "Lock screen", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't set it: " + ex.Message, "Lock screen", MessageBoxButton.OK);
            }
            finally
            {
                Busy(false);
            }
        }

        private void Busy(bool on)
        {
            Bar.Visibility = on ? Visibility.Visible : Visibility.Collapsed;
            Bar.IsIndeterminate = on;
        }

        private static Task<string> Download(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            var client = new WebClient();
            client.Headers["User-Agent"] = AppSettings.UserAgent;
            client.OpenReadCompleted += (s, e) =>
            {
                try
                {
                    if (e.Error != null) { tcs.SetException(e.Error); return; }
                    
                    string name = "lock" + DateTime.Now.Ticks + ".png";
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    using (var output = store.OpenFile(name, FileMode.Create))
                    using (e.Result)
                    {
                        e.Result.CopyTo(output);
                    }
                    tcs.SetResult(name);
                }
                catch (Exception ex) { tcs.SetException(ex); }
            };
            client.OpenReadAsync(new Uri(url));
            return tcs.Task;
        }

        private static void DeleteOldImages(string keep)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                foreach (var f in store.GetFileNames("lock*.png"))
                    if (f != keep) store.DeleteFile(f);
        }
    }
}