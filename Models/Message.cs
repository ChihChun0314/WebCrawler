using System;
using System.Collections.Generic;

namespace WebCrawler.Models
{
    public partial class Message
    {
        public int MesId { get; set; }
        public int? UId { get; set; }
        public string? Content { get; set; }
        public DateTime? Date { get; set; }
    }
}
