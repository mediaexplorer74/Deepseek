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
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using System.Collections.Generic;

namespace DeepseekUWPApp
{
    public sealed partial class MainPage : Page
    {
        private static readonly string API_URL = "https://openrouter.ai/api/v1/chat/completions";

        //private static readonly string RequestLimitSetting = "DailyRequestLimit";
        
        //private int _requestsMadeToday = 0;

        // Replace ObservableCollection with regular List + helper method
        private List<Message> _messages = new List<Message>();
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();
       

        // Replace static ApiKey with dynamic retrieval
        private string ApiKey
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["DeepseekApiKey"]?.ToString();
            }
        }
        
        

        private DispatcherTimer _autoSaveTimer; // Periodic Auto-Save

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            SetupAutoSave();
        }

        private void SetupAutoSave()
        {
            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _autoSaveTimer.Tick += async (s, e) => await SaveChatHistory();
            _autoSaveTimer.Start();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize converters
            Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
            Resources.Add("InverseBoolToVisibilityConverter", new InverseBoolToVisibilityConverter());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadChatHistory();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            await SaveChatHistory();
        }

        private async Task LoadChatHistory()
        {
            var savedMessages = await ChatStorageHelper.LoadChatAsync();
            _messages = savedMessages;

            Messages.Clear();
            foreach (var msg in savedMessages)
            {
                Messages.Add(msg);
            }
        }

        private async Task SaveChatHistory()
        {
            await ChatStorageHelper.SaveChatAsync(_messages);
        }



        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            if (Messages.Count == 0)
            {
                await new MessageDialog("No messages to share").ShowAsync();
                return;
            }

            // Prepare share text
            var dataPackage = new DataPackage();
            dataPackage.Properties.Title = "Chat Conversation";

            StringBuilder chatHistory = new StringBuilder();
            foreach (var message in Messages)
            {
                chatHistory.AppendLine($"[{message.Timestamp:HH:mm}] {(message.IsUserMessage ? "You" : "AI")}:");
                chatHistory.AppendLine(message.Content);
                chatHistory.AppendLine();
            }

            dataPackage.SetText(chatHistory.ToString());

            // Show share UI
            DataTransferManager.ShowShareUI(new ShareUIOptions { Theme = ShareUITheme.Dark });
            Clipboard.SetContent(dataPackage);
        }

        private void ShareSingleMessage(Message message)
        {
            var dataPackage = new DataPackage();
            dataPackage.Properties.Title = "Chat Message";
            dataPackage.SetText($"[{message.Timestamp:HH:mm}] {(message.IsUserMessage ? "You" : "AI")}:\n{message.Content}");

            DataTransferManager.ShowShareUI();
            Clipboard.SetContent(dataPackage);
        }
        /*
        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            if (Messages.Count == 0)
            {
                await new MessageDialog("No messages to share").ShowAsync();
                return;
            }

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (s, args) =>
            {
                DataRequest request = args.Request;
                request.Data.Properties.Title = "Chat Conversation";

                // Format chat history
                StringBuilder chatHistory = new StringBuilder();
                foreach (var message in Messages)
                {
                    chatHistory.AppendLine($"[{message.Timestamp:HH:mm}] {(message.IsUserMessage ? "You" : "AI")}:");
                    chatHistory.AppendLine(message.Content);
                    chatHistory.AppendLine();
                }

                request.Data.SetText(chatHistory.ToString());
            };

            DataTransferManager.ShowShareUI();
        }*/

        // Add right-click menu for individual messages
        private void ChatListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is Message message)
            {
                var flyout = new MenuFlyout();

                var shareItem = new MenuFlyoutItem { Text = "Share" };
                shareItem.Click += (s, args) => ShareSingleMessage(message);

                flyout.Items.Add(shareItem);
                flyout.ShowAt(element, e.GetPosition(element));
            }
        }

        /*
        private void ShareSingleMessage(Message message)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (s, args) =>
            {
                DataRequest request = args.Request;
                request.Data.Properties.Title = "Chat Message";
                request.Data.SetText($"[{message.Timestamp:HH:mm}] {(message.IsUserMessage ? "You" : "AI")}:\n{message.Content}");
            };

            DataTransferManager.ShowShareUI();
        }*/

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

        //private bool CheckRequestLimitExceeded()
        //{
        //    var limit = ApplicationData.Current.LocalSettings.Values[RequestLimitSetting] as int? ?? 30;
        //    return _requestsMadeToday >= limit;
        //}

        private async Task ProcessUserInput()
        {
            // check api key 
            if (string.IsNullOrEmpty(ApiKey))
            {
                Messages.Add(new Message { Content = "API key not set! Go to Settings.", IsUserMessage = false });
                return;
            }

            // check limits
            //if (CheckRequestLimitExceeded())
            //{
            //    Messages.Add(new Message { Content = "Daily request limit exceeded!", IsUserMessage = false });
            //    return;
            //}

            string inputText = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(inputText)) return;

            // When adding messages:
            var userMessage = new Message { Content = inputText, IsUserMessage = true };
            _messages.Add(userMessage);
            Messages.Add(userMessage);

            InputTextBox.Text = string.Empty;

            // Show loading
            LoadingProgress.Visibility = Visibility.Visible;
            LoadingProgress.IsActive = true;

            try
            {
                // Get bot response
                string botResponse = await GetBotResponse(inputText);

                // After bot response:
                var botMessage = new Message { Content = botResponse, IsUserMessage = false };
                _messages.Add(botMessage);
                Messages.Add(botMessage);

                //_requestsMadeToday++;
            }
            catch (Exception ex)
            {
                var botMessage = new Message { Content = $"Error: {ex.Message}", IsUserMessage = false };
                _messages.Add(botMessage);
                Messages.Add(botMessage);
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

            await SaveChatHistory(); // Auto-save after each message
        }//ProcessUserInput


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
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }//Settings_Click
        private void LimitsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LimitsPage));
        }//Limits_Click    

        // *** Action bar handling methods - END ***
    }//MainPage class end
}