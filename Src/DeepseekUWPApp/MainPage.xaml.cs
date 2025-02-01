using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json;

namespace DeepseekUWPApp
{
    public sealed partial class MainPage : Page
    {
        // Plan B
        private static readonly string API_URL = "https://openrouter.ai/api/v1/chat/completions";//"https://api.deepseek.com/v1/chat/completions";
        private static readonly string API_KEY = "xxxxxx"; // use your own OpenRouter key

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

        private async Task ProcessUserInput()
        {
            string inputText = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(inputText)) return;

            // Add user message
            Messages.Add(new Message { Content = inputText, IsUserMessage = true });
            InputTextBox.Text = string.Empty;

            try
            {
                // Get bot response
                string botResponse = await GetBotResponse(inputText);
                Messages.Add(new Message { Content = botResponse, IsUserMessage = false });
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Content = $"Error: {ex.Message}", IsUserMessage = false });
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
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");

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
        }
    }
}