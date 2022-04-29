using System;
using System.Collections.Generic;

namespace WebCrawler.Models
{
    public partial class Analysis
    {
        public int AnalysisId { get; set; }
        public int? CrawlerId { get; set; }
        public DateTime? Time { get; set; }
        public string? State { get; set; }
        public string? Comtent { get; set; }
        public int? TypeId { get; set; }
    }
}
