namespace Synerixis.Application.DTOs
{
    public class OptimizeProductResponse
    {
        public string optimizedtitle { get; set; } = string.Empty;
        public string optimizeddescription { get; set; } = string.Empty;  // Markdown 格式
        public List<string> imageoptimizationprompts { get; set; } = new();  // 图片生成 prompt
        public MarketingPlanDto marketingplan { get; set; } = new();
    }

    public class MarketingPlanDto
    {
        public string shortvideoScript { get; set; } = string.Empty;
        public string plantingtext { get; set; } = string.Empty;  // 种草文案
        public string livescript { get; set; } = string.Empty;    // 直播话术
        public List<string> keysellingpoints { get; set; } = new();
    }
}
