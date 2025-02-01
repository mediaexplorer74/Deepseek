// Main Page

using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

namespace DeepseekUWPApp
{
    public sealed partial class MainPage : Page
    {
        private static readonly string API_URL = "https://openrouter.ai/api/v1/chat/completions";

        private static readonly string RequestLimitSetting = "DailyRequestLimit";
        private int _requestsMadeToday = 0;

        // Replace static ApiKey with dynamic retrieval
        private string ApiKey
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["DeepseekApiKey"]?.ToString();
            }
        }
        
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize converters
            Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
            Resources.Add("InverseBoolToVisibilityConverter", new InverseBoolToVisibilityConverter());
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ProcessUserInput();
        }

        private async void InputTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(InputTextBox.Text))
            {
                await ProcessUserInput();
            }
        }

        private bool CheckRequestLimitExceeded()
        {
            var limit = ApplicationData.Current.LocalSettings.Values[RequestLimitSetting] as int? ?? 30;
            return _requestsMadeToday >= limit;
        }
        private async Task ProcessUserInput()
        {
            // check api key 
            if (string.IsNullOrEmpty(ApiKey))
            {
                Messages.Add(new Message { Content = "API key not set! Go to Settings.", IsUserMessage = false });
                return;
            }

            // check limits
            if (CheckRequestLimitExceeded())
            {
                Messages.Add(new Message { Content = "Daily request limit exceeded!", IsUserMessage = false });
                return;
            }

            string inputText = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(inputText)) return;

            // Add user message
            Messages.Add(new Message { Content = inputText, IsUserMessage = true });
            InputTextBox.Text = string.Empty;

            // Show loading
            LoadingProgress.Visibility = Visibility.Visible;
            LoadingProgress.IsActive = true;

            try
            {
                // Get bot response
                string botResponse = await GetBotResponse(inputText);
                Messages.Add(new Message { Content = botResponse, IsUserMessage = false });

                _requestsMadeToday++;
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Content = $"Error: {ex.Message}", IsUserMessage = false });
            }
            finally
            {
                // Hide loading
                LoadingProgress.Visibility = Visibility.Collapsed;
                LoadingProgress.IsActive = false;
            }


            // Scroll to the latest message
            if (ChatListView.Items.Count > 0)
            {
                ChatListView.ScrollIntoView(ChatListView.Items[ChatListView.Items.Count - 1]);
            }
        }

        private async Task<string> GetBotResponse(string inputText)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
                //client.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/mediaexplorer74/Deepseek"); // Required by OpenRouter
                //client.DefaultRequestHeaders.Add("X-Title", "DeepseekUWPApp"); // Optional

                var requestBody = new
                {
                    model = "deepseek/deepseek-r1:free",//"deepseek-chat",
                    messages = new[]
                    {
                        new { role = "user", content = inputText }
                    }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(API_URL, content);
                response.EnsureSuccessStatusCode();

                string responseJson = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseJson);

                // Extract the bot's reply (adjust based on API response structure)
                return responseData.choices[0].message.content;
            }
        }// GetBotResponse


        // *** Action bar handling methods - BEGIN ***
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }//Settings_Click
        private void Limits_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LimitsPage));
        }//Limits_Click    

        // *** Action bar handling methods - END ***
    }//MainPage class end
}