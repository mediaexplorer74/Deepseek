using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeepSeekVS
{
    public partial class ChatWindowControl : UserControl
    {
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();
        private bool _isLoading;

        public ChatWindowControl()
        {
            InitializeComponent();
            ChatHistory.ItemsSource = Messages;
        }

        private async void SendMessage()
        {
            string apikey = "...";
            string ai_model = "deepseek/deepseek-r1:free";
            try
            {
                _isLoading = true;
                OptionsPage options = default;

                // TODO: fix this to use the correct options!!! now options is *null* :(
                options = (OptionsPage)Package.GetGlobalService(typeof(OptionsPage));

                if (options != null)
                {
                    apikey = options.ApiKey;
                    ai_model = options.Model;
                }               

                if (string.IsNullOrEmpty(apikey))
                {
                    MessageBox.Show("API key is not set in settings!");
                    return;
                }

                var userMessage = new ChatMessage(InputBox.Text, true);
                Messages.Add(userMessage);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apikey);

                    var request = new
                    {
                        model = ai_model,
                        messages = new[]
                        {
                            new { role = "user", content = InputBox.Text }
                        }
                    };

                    var response = await client.PostAsync(
                        "https://openrouter.ai/api/v1/chat/completions",
                        new StringContent(JsonConvert.SerializeObject(request),
                            System.Text.Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"API Error: {response.ReasonPhrase}");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(content);

                    Messages.Add(new ChatMessage(
                        (string)result.choices[0].message.content,
                        false));
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage($"Error: {ex.Message}", false, true));
            }
            finally
            {
                _isLoading = false;
                InputBox.Clear();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                SendMessage();
                e.Handled = true;
            }
        }
    }

    public class ChatMessage
    {
        public string Content { get; }
        public bool IsUser { get; }
        public bool IsError { get; }
        public HorizontalAlignment Alignment => IsUser 
            ? HorizontalAlignment.Right
            : HorizontalAlignment.Left;
        public System.Windows.Media.Brush BubbleBackground => IsError
            ? System.Windows.Media.Brushes.IndianRed
            : (IsUser ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.White);

        public ChatMessage(string content, bool isUser, bool isError = false)
        {
            Content = content;
            IsUser = isUser;
            IsError = isError;
        }
    }
}