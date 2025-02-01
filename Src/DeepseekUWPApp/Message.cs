// Message Model Class

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System;

namespace DeepseekUWPApp
{
    public class Message
    {
        public string Content { get; set; }
        public bool IsUserMessage { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
