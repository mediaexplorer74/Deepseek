// Limits Page

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace DeepseekUWPApp
{
    public sealed partial class LimitsPage : Page
    {
        //private const string RequestLimitSetting = "DailyRequestLimit";
        private const string OpenRouterLimitsUrl = "https://openrouter.ai/api/v1/auth/key";

        public LimitsPage()
        {
            InitializeComponent();
            //LoadLimit();
            ShowApiKeyLimits();
        }

        //private void LoadLimit()
        //{
        //    LimitTextBox.Text = ApplicationData.Current.LocalSettings.Values[RequestLimitSetting]?.ToString() ?? "30";
        //}

        //private void SetLimit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (int.TryParse(LimitTextBox.Text, out int limit))
        //    {
        //        ApplicationData.Current.LocalSettings.Values[RequestLimitSetting] = limit;
        //    }
        //}


        // show API key limit

        private async void ShowApiKeyLimits()//CheckApiLimits_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = ApplicationData.Current.LocalSettings.Values["DeepseekApiKey"]?.ToString();

            if (string.IsNullOrEmpty(apiKey))
            {
                ApiLimitsText.Text = "API key not set!";
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var response = await client.GetAsync(OpenRouterLimitsUrl);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var limits = JsonConvert.DeserializeObject<OpenRouterLimits>(json);

                    ApiLimitsText.Text = 
                        $"Label: {limits.data.label}\n" +
                        $"Limit: {limits.data.limit}\n" +
                        $"Remaining Limit: {limits.data.limit_remaining}\n" +
                        $"Is free tier: {limits.data.is_free_tier}\n" +
                        $"Rate limit - requests: {limits.data.rate_limit.requests}\n" +
                        $"Rate limit - interval: {limits.data.rate_limit.interval}";
                }
            }
            catch (Exception ex)
            {
                ApiLimitsText.Text = $"Error fetching limits: {ex.Message}";
            }
        }


        // *** Action bar handling methods - BEGIN ***
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }//Settings_Click
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }//Home_Click    

        // *** Action bar handling methods - END ***

    }//LimitsPage class end


    // OpenRouterLimits class
    public class OpenRouterLimits
    {
        //public string usage { get; set; }
        //public string limit { get; set; }
        //public string reset { get; set; }
        public LimitsData data { get; set; }
    }

    public class LimitsData
    {
        public string label { get; set; }
        public string limit { get; set; }
        public int usage { get; set; }
        public string limit_remaining { get; set; }
        public bool is_free_tier { get; set; }

        public RateLimitData rate_limit { get; set; }
    }

    public class RateLimitData
    {
        public int requests { get; set; }
        public string interval { get; set; }    
    }
}