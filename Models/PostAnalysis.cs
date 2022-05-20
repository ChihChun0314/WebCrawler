using System;
using System.Collections.Generic;

namespace WebCrawler.Models
{
    public partial class PostAnalysis
    {
        public int AId { get; set; }
        public int? CId { get; set; }
        public int? TId { get; set; }
        public string? Content { get; set; }
        public DateTime? Time { get; set; }
        public string? Url { get; set; }
        public string? WebName { get; set; }
    }
}
