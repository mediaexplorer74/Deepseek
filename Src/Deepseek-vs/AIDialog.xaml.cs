using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly string ApiKey = "sk-or-v1-..."; // paste your openrouter.ai api key here

        private static readonly string API_URL = "https://openrouter.ai/api/v1/chat/completions";


        public AIDialog()
        {
            InitializeComponent();
        }

       
        // SendButton_Click
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic result = "::";//null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
               
                var requestBody = new
                {
                    model = "deepseek/deepseek-r1:free",
                    messages = new[]
                    {
                        new { role = "user", content = UserInput.Text }
                    }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                try
                {
                    response = await client.PostAsync(API_URL, content);
                    //response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
                }

                string responseJson = "";
                dynamic responseData = null;

                if (response != null)
                {
                    responseJson = await response.Content.ReadAsStringAsync();
                    responseData = JsonConvert.DeserializeObject(responseJson);
                }


                if (responseData != null)
                    result = responseData.choices[0].message.content;
               
            }
            AIResponse.Text = result;

        }//SendButton_Click

        // PastedButton_Click
        private void PastedButton_Click(object sender, RoutedEventArgs e)
        {
            var codeBlock = ExtractCodeFromResponse(AIResponse.Text);
            if (!string.IsNullOrEmpty(codeBlock))
            {
                InsertCodeIntoEditor(codeBlock); // Вставка кода в редактор
            }
        }// PastedButton_Click

        private void InsertCodeIntoEditor(string code)
        {
            // Получение активного документа
            var dte = (EnvDTE.DTE)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE));
            var activeDocument = dte.ActiveDocument;

            if (activeDocument != null)
            {
                // Вставка кода в текущую позицию курсора
                var selection = (EnvDTE.TextSelection)activeDocument.Selection;
                selection.Insert(code, (int)(EnvDTE.vsInsertFlags.vsInsertFlagsInsertAtStart));
            }
        }

        private string ExtractCodeFromResponse(string response)
        {
            // Пример: поиск текста между маркерами ```code и ```
            int start = response.IndexOf("```code");
            int end = response.IndexOf("```", start + 7);

            if (start != -1 && end != -1)
            {
                return response.Substring(start + 7, end - start - 7).Trim();
            }

            return response.Trim(); // Если маркеры отсутствуют, вставить весь ответ
        }
    }
}
