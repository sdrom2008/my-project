using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class SellerConfigUpdateDto
    {
        public string? ShopName { get; set; }
        public string? ShopLogo { get; set; }
        public string? MainCategory { get; set; }
        public string? TargetCustomerDesc { get; set; }
        public string? DefaultReplyTone { get; set; }
        public string? PreferredLanguage { get; set; }
        public bool EnableAutoMarketingReminder { get; set; }
        public int MemoryRetentionDays { get; set; }
    }
}
