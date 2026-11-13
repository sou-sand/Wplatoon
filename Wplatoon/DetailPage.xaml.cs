using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Wplatoon
{
    public partial class DetailPage : PhoneApplicationPage
    {
        public DetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = DataSource.Selected;
        }
    }
}