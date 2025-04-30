using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenRouterAIExtension
{
    /// <summary>
    /// Interaction logic for AIDialog.xaml
    /// </summary>
    public partial class AIDialog : Window
    {
        //TODO : options
        private static readonly string ApiKey = "sk-or-v1-............."; // paste your openrouter.ai api key here

        private static readonly string API_URL = "https://openrouter.ai/api/v1/chat/completions";


        public AIDialog()
        {
            InitializeComponent();
        }

       
        // SendButton_Click
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            /*using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

                var payload = new
                {
                    model = "deepseek/deepseek-r1:free",
                    prompt = UserInput.Text
                    //messages = new[]
                    //{
                    //    new { role = "user", content = UserInput.Text }
                    //}

                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8,
                    "application/json");
                var response = await client.PostAsync("https://openrouter.ai/api/v1/completions",
                    content);

                var result = await response.Content.ReadAsStringAsync();
                AIResponse.Text = result; // Parse JSON if needed
            }*/
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
               
                var requestBody = new
                {
                    model = "deepseek/deepseek-r1:free",//"deepseek-chat",
                    messages = new[]
                    {
                        new { role = "user", content = UserInput.Text }
                    }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(API_URL, content);
                response.EnsureSuccessStatusCode();

                string responseJson = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseJson);

                // Extract the bot's reply (adjust based on API response structure)
                var result = responseData.choices[0].message.content;

                AIResponse.Text = result;
            }
            

        }//SendButton_Click
    }
}
