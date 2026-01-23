using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class OptimizeProductData
    {
        public string OptimizedTitle { get; set; } = string.Empty;
        public string OptimizedDescription { get; set; } = string.Empty;
        public MarketingPlanData MarketingPlan { get; set; } = new();
        public List<string> ImagePrompts { get; set; } = new();
    }

    public class MarketingPlanData
    {
        public string ShortVideoScript { get; set; } = string.Empty;
        public string PlantingText { get; set; } = string.Empty;
        public string LiveScript { get; set; } = string.Empty;
        public List<string> KeySellingPoints { get; set; } = new();
    }
}
