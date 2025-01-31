using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//dotnet add package Azure.AI.Inference --prerelease
using Azure;
using Azure.AI.Inference;


namespace DeepseekConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // To authenticate with the model you will need to generate a personal access token (PAT) in your GitHub settings. 
            // Create your PAT token by following instructions here: https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens
            AzureKeyCredential credential = new AzureKeyCredential
            (
              "xxxxxx"
            );

            ChatCompletionsClient client = new ChatCompletionsClient(
                new Uri("https://models.inference.ai.azure.com"),
                credential,
                new ChatCompletionsClientOptions());

            var requestOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatRequestUserMessage(
                        "Can you explain the basics of machine learning?"),
                },
                Model = "DeepSeek-R1",
                MaxTokens = 2048,

            };

            Response<ChatCompletions> response = client.Complete(requestOptions);
            var rcontent = response.Value.Choices[0].Message.Content;
            System.Console.WriteLine(rcontent);//.Choices[0].Message.Content);
            Debug.WriteLine("Response content: " + rcontent);

        }
    }
}
