using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wplatoon
{
    public partial class HomePage : PhoneApplicationPage
    {
        private const string SchedulesUrl = "https://splatoon3.ink/data/schedules.json"; // to-do: switch between splatoon3.ink and splatoon2.ink
        private const string CacheName = "schedules.json";

        private bool _downloading;
        private bool _hasData;
        private DateTimeOffset? _rotationEnd;
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        public HomePage()
        {
            InitializeComponent();
            _timer.Tick += Timer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadSchedules();
            _timer.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_rotationEnd.HasValue && DateTimeOffset.UtcNow >= _rotationEnd.Value)
            {
                LoadSchedules();
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

        // private void AddButton_Click(object sender, EventArgs e) { }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadSchedules();
        }
        
        private void LoadSchedules()
        {
            string cached = CacheHelper.Load(CacheName);
            if (cached != null && ShowSchedules(cached) && CacheHelper.IsFresh(CacheName))
            {
                StatusText.Text = "Updated " + CacheHelper.GetTime(CacheName).Value.ToString("HH:mm")
                                  + " (data refreshes hourly)";
                return;
            }
            Download();
        }

        private void Download()
        {
            if (_downloading) return;
            _downloading = true;

            LoadingBar.Visibility = Visibility.Visible;
            LoadingBar.IsIndeterminate = true;
            StatusText.Text = "Updating...";

            var client = new WebClient();
            client.Headers["User-Agent"] = "Wplatoon/1.0";
            client.DownloadStringCompleted += Client_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri(SchedulesUrl));
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _downloading = false;
            LoadingBar.IsIndeterminate = false;
            LoadingBar.Visibility = Visibility.Collapsed;

            if (e.Error != null)
            {
                StatusText.Text = _hasData
                    ? "Can't update right now, showing saved data"
                    : "Couldn't load data. Check your connection and refresh.";
                return;
            }

            if (ShowSchedules(e.Result))
            {
                CacheHelper.Save(CacheName, e.Result);
                StatusText.Text = "Updated " + DateTime.Now.ToString("HH:mm");
            }
            else
            {
                StatusText.Text = _hasData
                    ? "Got unexpected data, showing saved data"
                    : "Got unexpected data from the server";
            }
        }
        
        private bool ShowSchedules(string json)
        {
            try
            {
                JObject root;
                using (var reader = new JsonTextReader(new StringReader(json)))
                {
                    reader.DateParseHandling = DateParseHandling.None;
                    root = JObject.Load(reader);
                }

                var turf = ScheduleParser.TurfWar(root);
                ItemsListBox.ItemsSource = turf;
                AnarchyListBox.ItemsSource = ScheduleParser.Anarchy(root);
                XListBox.ItemsSource = ScheduleParser.XBattles(root);
                SalmonListBox.ItemsSource = ScheduleParser.SalmonRun(root);

                _rotationEnd = ScheduleParser.CurrentRotationEnd(root);
                UpdateCountdown();

                TileHelper.Update(turf.Count > 0 ? turf[0] : null);

                _hasData = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}