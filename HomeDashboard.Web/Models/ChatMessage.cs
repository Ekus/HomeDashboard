using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeDashboard.Web
{
    public class ChatMessage
    {
        public ChatMessage()
        {
            Timestamp = DateTime.Now;
        }

        public string AuthorId { get; set;}
        public string Text { get; set;}
        public dynamic AuthorMetadata { get; set; }
      
        public DateTime Timestamp
        {
            get;
            set;
        }
    }
}

