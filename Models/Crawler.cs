﻿using System;
using System.Collections.Generic;

namespace WebCrawler.Models
{
    public partial class Crawler
    {
        public int CId { get; set; }
        public int? UId { get; set; }
        public string? Content { get; set; }
        public DateTime? Time { get; set; }
        public string? Url { get; set; }
        public string? WebName { get; set; }
    }
}
