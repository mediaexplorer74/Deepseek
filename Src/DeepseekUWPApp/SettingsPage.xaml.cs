// Settings Page

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;

namespace DeepseekUWPApp
{
    public sealed partial class SettingsPage : Page
    {
        private const string ApiKeySetting = "DeepseekApiKey";

        public SettingsPage()
        {
            InitializeComponent();
            LoadApiKey();
        }

        private void LoadApiKey()
        {
            ApiKeyBox.Password = ApplicationData.Current.LocalSettings.Values[ApiKeySetting]?.ToString() ?? "";
        }

        private void SaveApiKey_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[ApiKeySetting] = ApiKeyBox.Password;
        }

        // *** Action bar handling methods - BEGIN ***
        private void Limits_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LimitsPage));
        }//Limits_Click
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }//Home_Click    

        // *** Action bar handling methods - END ***
    }
}
