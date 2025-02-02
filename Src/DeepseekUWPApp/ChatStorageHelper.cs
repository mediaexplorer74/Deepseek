using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace DeepseekUWPApp
{
    public static class ChatStorageHelper
    {
        private const string ChatFileName = "chatHistory.json";

        public static async Task SaveChatAsync(IEnumerable<Message> messages)
        {
            var data = new ChatData { Messages = new List<Message>(messages) };
            string json = JsonConvert.SerializeObject(data);

            StorageFile file = await ApplicationData.Current.LocalFolder
                .CreateFileAsync(ChatFileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, json);
        }

        public static async Task<List<Message>> LoadChatAsync()
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(ChatFileName);
                string json = await FileIO.ReadTextAsync(file);
                var data = JsonConvert.DeserializeObject<ChatData>(json);
                return data?.Messages ?? new List<Message>();
            }
            catch
            {
                return new List<Message>();
            }
        }

        private class ChatData
        {
            public List<Message> Messages { get; set; }
        }
    }
}
