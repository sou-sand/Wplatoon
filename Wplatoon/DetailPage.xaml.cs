using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using System.Windows;

namespace Wplatoon
{
    public partial class DetailPage : PhoneApplicationPage
    {
        public DetailPage()
        {
            InitializeComponent();
            var sel = DataSource.Selected;
            DateTime? t = RemindTime(DataSource.Selected);
            RemindButton.Visibility = (t.HasValue && t.Value > DateTime.Now) ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = DataSource.Selected;
        }

        private static DateTime? RemindTime(ItemViewModel item)
        {
            if (item == null) return null;
            if (item.RemindAt.HasValue) return item.RemindAt;
            if (item.StartTime.HasValue) return item.StartTime.Value.AddMinutes(-5);
            return null;
        }

        private void RemindButton_Click(object sender, RoutedEventArgs e)
        {
            var item = DataSource.Selected;
            DateTime? target = RemindTime(item);
            if (item == null || !target.HasValue) return;

            DateTime when = target.Value;
            if (when <= DateTime.Now)
            {
                MessageBox.Show("This one is starting too soon for a reminder.", "Wplatoon", MessageBoxButton.OK);
                return;
            }

            // ask before setting it
            var answer = MessageBox.Show(
                "Remind you at " + when.ToString("ddd " + AppSettings.TimeFormat) + "?\n\n" + item.Title + ": " + item.Description,
                "Set reminder", MessageBoxButton.OKCancel);
            if (answer != MessageBoxResult.OK) return;

            // names must be unique and at most 63 characters
            string name = "wp" + when.Ticks + "-" + Math.Abs((item.Title ?? "").GetHashCode());

            try
            {
                if (ScheduledActionService.Find(name) != null) ScheduledActionService.Remove(name);

                ScheduledActionService.Add(new Reminder(name)
                {
                    Title = "Wplatoon",
                    Content = item.Title + ": " + item.Description,
                    BeginTime = when,
                    ExpirationTime = when.AddHours(2),
                    RecurrenceType = RecurrenceInterval.None,
                    NavigationUri = new Uri("/HomePage.xaml", UriKind.Relative)
                });
                MessageBox.Show("Reminder set for " + when.ToString("ddd " + AppSettings.TimeFormat) + ".",
                    "Reminder set", MessageBoxButton.OK);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("The phone allows about 50 reminders per app, and that limit has been reached.",
                    "Couldn't add reminder", MessageBoxButton.OK);
            }
        }
    }
}