using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;

namespace DeepseekUWPApp
{
    public sealed partial class MainPage : Page
    {
        // Plan B
        private static readonly string API_URL = "https://openrouter.ai/api/v1/chat/completions";//"https://api.deepseek.com/v1/chat/completions";
        private static readonly string API_KEY = "sk-or-v1-xxxxxxxxxxxxxxxxxxxxxxxx"; // use your own OpenRouter key

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputTextBox.Text;
            string response = await GetDeepSeekResponse(userInput);
            ResponseTextBlock.Text = response;
        }

        private async Task<string> GetDeepSeekResponse(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");

                var requestBody = new
                {
                    model = "deepseek/deepseek-r1:free",//"deepseek-chat",
                    messages = new[]
                    {
                        //new { role = "system", content = "deepseek" },
                        new { role = "user", content = prompt }
                    }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(API_URL, content);
                string result = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(result);
                return jsonResponse?.choices?[0]?.message?.content ?? "Error of API response";
            }
        }
    }
}
