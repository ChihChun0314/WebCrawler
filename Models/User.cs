using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models
{
    public partial class User
    {
        public int UId { get; set; }
        public string? UEmail { get; set; }
        public string? UPassword { get; set; }
        public string? UName { get; set; }
        public string? Permission { get; set; }
        [StringLength(10, MinimumLength = 10, ErrorMessage = "必須輸入10個字")]
        public string? PhoneNumber { get; set; }
    }
}
