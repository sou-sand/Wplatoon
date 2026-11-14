using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Scheduler;

namespace Wplatoon
{
    public partial class HomePage : PhoneApplicationPage
    {
        private readonly HashSet<string> _busy = new HashSet<string>();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        private bool _hasData;
        private bool _festsRequested;
        private string _note = "";
        private DateTimeOffset? _rotationEnd;
        private List<ItemViewModel> _salmon = new List<ItemViewModel>();
        private ItemViewModel _reward;

        public HomePage()
        {
            InitializeComponent();
            _timer.Tick += Timer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplyGame();
            LoadAll();
            _timer.Start();
        }

        private static bool _agentChecked;
        private const string AgentName = "WplatoonTileAgent";

        private static void EnsureAgent()
        {
            if (_agentChecked) return;
            _agentChecked = true;

            try
            {
                // remove and re-add on each launch, which renews the 2-week expiration
                if (ScheduledActionService.Find(AgentName) != null)
                    ScheduledActionService.Remove(AgentName);

                ScheduledActionService.Add(new PeriodicTask(AgentName)
                {
                    Description = "Keeps the live tile on the current maps.",
                    ExpirationTime = DateTime.Now.AddDays(14)
                });
                #if DEBUG
                    ScheduledActionService.LaunchForTest(AgentName, TimeSpan.FromSeconds(10));
                #endif
            }
            catch (InvalidOperationException) { }
            catch (SchedulerServiceException) { }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _timer.Stop();
        }
        

        private void ChangeSplatoon_Click(object sender, EventArgs e)
        {
            AppSettings.Game = AppSettings.Game == 3 ? 2 : 3;
            
            ItemsListBox.ItemsSource = null;
            AnarchyListBox.ItemsSource = null;
            XListBox.ItemsSource = null;
            SalmonListBox.ItemsSource = null;
            GearListBox.ItemsSource = null;
            FestListBox.ItemsSource = null;
            _salmon = new List<ItemViewModel>();
            _reward = null;
            _rotationEnd = null;
            _hasData = false;
            _festsRequested = false;
            _note = "";
            CountdownText.Text = "";

            ApplyGame();
            LoadAll();
        }
        
        private void ApplyGame()
        {
            bool s3 = AppSettings.Game == 3;
            MainPanorama.Title = s3 ? "wplatoon" : "wplatoon 2";
            AnarchyItem.Header = s3 ? "anarchy" : "ranked";
            XItem.Header = s3 ? "x battles" : "league";
            SetItem(GearItem, s3);
            SetItem(FestItem, s3);
        }

        private void SetItem(PanoramaItem item, bool show)
        {
            bool present = MainPanorama.Items.Contains(item);
            if (show && !present) MainPanorama.Items.Add(item);
            else if (!show && present) MainPanorama.Items.Remove(item);
        }
        

        private void LoadAll()
        {
            Load(AppSettings.SchedulesCache, AppSettings.SchedulesUrl, ShowSchedules, true);

            if (AppSettings.Game == 3)
            {
                Load(AppSettings.Gear3Cache, AppSettings.Gear3Url, ShowGear, false);
                Load(AppSettings.Coop3Cache, AppSettings.Coop3Url, ShowCoop3, false);
                if (_festsRequested) LoadFests();
            }
            else
            {
                Load(AppSettings.Coop2Cache, AppSettings.Coop2Url, ShowCoop2, false);
            }
        }
        
        private void LoadFests()
        {
            _festsRequested = true;
            Load(AppSettings.Fest3Cache, AppSettings.Fest3Url, ShowFests, false);
        }

        private void MainPanorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppSettings.Game == 3 && MainPanorama.SelectedItem == FestItem && !_festsRequested)
                LoadFests();
        }
        
        private void Load(string name, string url, Func<string, bool> show, bool primary)
        {
            string cached = CacheHelper.Load(name);
            if (cached != null && show(cached) && CacheHelper.IsFresh(name))
            {
                if (primary)
                    SetStatus("updated " + CacheHelper.GetTime(name).Value.ToString("HH:mm") + _note);
                return;
            }
            Download(name, url, show, primary);
        }

        private void Download(string name, string url, Func<string, bool> show, bool primary)
        {
            if (!_busy.Add(name)) return;

            if (primary)
            {
                LoadingBar.Visibility = Visibility.Visible;
                LoadingBar.IsIndeterminate = true;
                SetStatus("updating...");
            }

            var client = new WebClient();
            client.Headers["User-Agent"] = AppSettings.UserAgent;
            client.DownloadStringCompleted += (s, e) =>
            {
                _busy.Remove(name);
                if (primary)
                {
                    LoadingBar.IsIndeterminate = false;
                    LoadingBar.Visibility = Visibility.Collapsed;
                }

                if (!IsCurrent(name)) return; 

                if (e.Error != null)
                {
                    if (primary)
                        SetStatus(_hasData ? "can't update right now, showing saved data"
                                           : "couldn't load data, check your connection and refresh");
                    return;
                }

                if (show(e.Result))
                {
                    CacheHelper.Save(name, e.Result);
                    if (primary) SetStatus("updated " + DateTime.Now.ToString("HH:mm") + _note);
                }
                else if (primary)
                {
                    SetStatus(_hasData ? "unexpected data, showing saved data"
                                       : "unexpected data from the server");
                }
            };
            client.DownloadStringAsync(new Uri(url));
        }

        private static bool IsCurrent(string name)
        {
            if (name == AppSettings.SchedulesCache) return true;
            if (AppSettings.Game == 2) return name == AppSettings.Coop2Cache;
            return name == AppSettings.Gear3Cache || name == AppSettings.Coop3Cache
                || name == AppSettings.Fest3Cache;
        }

        private void SetStatus(string text)
        {
            StatusText.Text = AppSettings.GameName + " - " + text;
        }

        private static JObject ParseJson(string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.DateParseHandling = DateParseHandling.None;
                return JObject.Load(reader);
            }
        }
        

        private bool ShowSchedules(string json)
        {
            try
            {
                JObject root = ParseJson(json);
                List<ItemViewModel> turf;

                if (AppSettings.Game == 2)
                {
                    turf = Splatoon2Parser.TurfWar(root);
                    AnarchyListBox.ItemsSource = Splatoon2Parser.Ranked(root);
                    XListBox.ItemsSource = Splatoon2Parser.League(root);
                    _rotationEnd = Splatoon2Parser.CurrentRotationEnd(root);
                    _note = turf.Count == 0 ? " - no current rotations in the data" : "";
                }
                else
                {
                    turf = ScheduleParser.TurfWar(root);
                    AnarchyListBox.ItemsSource = ScheduleParser.Anarchy(root);
                    XListBox.ItemsSource = ScheduleParser.XBattles(root);
                    _salmon = ScheduleParser.SalmonRun(root);
                    RebuildSalmon();
                    _rotationEnd = ScheduleParser.CurrentRotationEnd(root);
                    _note = "";
                }

                ItemsListBox.ItemsSource = turf;
                UpdateCountdown();
                TileHelper.Update(turf.Count > 0 ? turf[0] : null);
                if (TileHelper.LastError.Length > 0) _note += " - tile: " + TileHelper.LastError;

                _hasData = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ShowGear(string json)
        {
            try
            {
                GearListBox.ItemsSource = GearParser.Gear(ParseJson(json));
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool ShowCoop3(string json)
        {
            try
            {
                _reward = GearParser.SalmonReward(ParseJson(json));
                RebuildSalmon();
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool ShowCoop2(string json)
        {
            try
            {
                _salmon = Splatoon2Parser.SalmonRun(ParseJson(json));
                RebuildSalmon();
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool ShowFests(string json)
        {
            try
            {
                FestListBox.ItemsSource = GearParser.Splatfests(ParseJson(json), AppSettings.RegionCode, 12);
                return true;
            }
            catch (Exception) { return false; }
        }

        // Salmon Run list = monthly reward gear (Splatoon 3 only) + the rotations
        private void RebuildSalmon()
        {
            var list = new List<ItemViewModel>();
            if (_reward != null && AppSettings.Game == 3) list.Add(_reward);
            list.AddRange(_salmon);
            SalmonListBox.ItemsSource = list;
        }
        

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_rotationEnd.HasValue && DateTimeOffset.UtcNow >= _rotationEnd.Value)
            {
                LoadAll();
                return;
            }
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            if (!_rotationEnd.HasValue) { CountdownText.Text = ""; return; }

            TimeSpan left = _rotationEnd.Value - DateTimeOffset.UtcNow;
            if (left < TimeSpan.Zero) left = TimeSpan.Zero;

            CountdownText.Text = "Next rotation in " + (left.TotalHours >= 1
                ? (int)left.TotalHours + "h " + left.Minutes.ToString("00") + "m"
                : left.Minutes + ":" + left.Seconds.ToString("00"));
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            var item = list.SelectedItem as ItemViewModel;
            if (item == null) return;

            DataSource.Selected = item;
            list.SelectedItem = null;

            NavigationService.Navigate(new Uri("/DetailPage.xaml", UriKind.Relative));
        }

        private void RefreshButton_Click(object sender, EventArgs e) { LoadAll(); }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void Wallpapers_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/WallpapersPage.xaml", UriKind.Relative));
        }
    }
}