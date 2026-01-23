namespace MyProject.Application.DTOs
{
    public class OptimizeProductResponse
    {
        public string OptimizedTitle { get; set; } = string.Empty;
        public string OptimizedDescription { get; set; } = string.Empty;  // Markdown 格式
        public List<string> ImageOptimizationPrompts { get; set; } = new();  // 图片生成 prompt
        public MarketingPlanDto MarketingPlan { get; set; } = new();
    }

    public class MarketingPlanDto
    {
        public string ShortVideoScript { get; set; } = string.Empty;
        public string PlantingText { get; set; } = string.Empty;  // 种草文案
        public string LiveScript { get; set; } = string.Empty;    // 直播话术
        public List<string> KeySellingPoints { get; set; } = new();
    }
}
