using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models
{
    public partial class Announment
    {
        public int AnnoId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        
        public DateTime? Date { get; set; }
    }
}
