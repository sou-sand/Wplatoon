using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Wplatoon
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            RegionPicker.ItemsSource = AppSettings.RegionNames;
            LanguagePicker.ItemsSource = AppSettings.LanguageNames;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Clock24Switch.IsChecked = AppSettings.Use24Hour;
            RegionPicker.SelectedIndex = AppSettings.RegionIndex;
            LanguagePicker.SelectedIndex = AppSettings.LanguageIndex;
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            AppSettings.Use24Hour = Clock24Switch.IsChecked == true;
            if (RegionPicker.SelectedIndex >= 0)
                AppSettings.RegionIndex = RegionPicker.SelectedIndex;
            if (LanguagePicker.SelectedIndex >= 0)
                AppSettings.LanguageIndex = LanguagePicker.SelectedIndex;
        }
    }
}