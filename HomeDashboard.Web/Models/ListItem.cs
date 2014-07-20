using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeDashboard.Web
{
    public class ListItem
    {
        public ListItem()
        {
            Timestamp = DateTime.Now;
            Id = Guid.NewGuid();
            IsBeingEditedBy = new List<string>();
            IsBeingCompletedBy = new List<string>();
            Tags = new List<string>();
        }

        public Guid Id { get; set; }
        public string Text { get; set;}
        public List<string> IsBeingEditedBy { get; set; }
        public List<string> IsBeingCompletedBy { get; set; }
        public string CompletedBy { get; set; }
        public List<string> Tags {get;set;} //store name, category
        public Boolean IsUrgent { get; set; }

        public DateTime Timestamp
        {
            get;
            set;
        }
    }
}

