using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeDashboard.Web
{
    public class ListOfItems
    {
        public ListOfItems()
        {
            Items = new List<ListItem>();
        }

        public string Id { get; set; }

        public List<ListItem> Items { get; set; }
    }
}

