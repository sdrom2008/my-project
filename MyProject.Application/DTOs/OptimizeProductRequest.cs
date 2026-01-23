namespace MyProject.Application.DTOs
{
    public class OptimizeProductRequest
    {
        public string Intent { get; set; } = string.Empty;          // 用户一句话："帮我优化这款女装T恤详情，写营销方案，图片优化"
        public string? OriginalTitle { get; set; }
        public string? OriginalDescription { get; set; }
        public List<string>? OriginalImageUrls { get; set; } = new();  // 前端上传后返回的 URL
        public string? Category { get; set; }                   // 类目：女装/美妆等
        public string? TargetPlatform { get; set; } = "Taobao"; // Taobao/Pinduoduo/Douyin
    }
}