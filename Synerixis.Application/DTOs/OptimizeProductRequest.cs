namespace Synerixis.Application.DTOs
{
    public class OptimizeProductRequest
    {
        public string intent { get; set; } = string.Empty;          // 用户一句话："帮我优化这款女装T恤详情，写营销方案，图片优化"
        public string? originaltitle { get; set; }
        public string? originaldescription { get; set; }
        public List<string>? originalimageurls { get; set; } = new();  // 前端上传后返回的 URL
        public string? category { get; set; }                   // 类目：女装/美妆等
        public string? targetplatform { get; set; } = "Taobao"; // Taobao/Pinduoduo/Douyin
    }
}