using System;
using System.Collections.Generic;

namespace WebCrawler.Models
{
    public partial class Crawler
    {
        public int CrawlerId { get; set; }
        public int UserId { get; set; }
        public DateTime Time { get; set; }
        public string WebCrawler { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
