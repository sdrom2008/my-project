using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class OptimizeProductData
    {
        public string optimizedtitle { get; set; } = string.Empty;
        public string optimizeddescription { get; set; } = string.Empty;
        public MarketingPlanData marketingplandto { get; set; } = new();
        public List<string> imageprompts { get; set; } = new();
    }

    public class MarketingPlanData
    {
        public string shortvideoscript { get; set; } = string.Empty;
        public string plantingtext { get; set; } = string.Empty;
        public string livescript { get; set; } = string.Empty;
        public List<string> keysellingpoints { get; set; } = new();
    }
}
