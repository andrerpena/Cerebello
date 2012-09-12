using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Chat
{
    public class ChatMessage
    {
        public ChatUser UserFrom { get; set; }
        public ChatUser UserTo { get; set; }
        public long Timestamp { get; set; }
        public string Message { get; set; }
    }
}