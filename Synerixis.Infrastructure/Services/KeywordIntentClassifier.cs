using Synerixis.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Infrastructure.Services
{
    public class KeywordIntentClassifier : IIntentClassifier
    {
        private readonly List<(string Intent, string[] Keywords)> _rules = new()
        {
            ("商品优化", new[] { "优化", "详情", "标题", "描述", "详情页" }),
            ("营销文案", new[] { "营销", "文案", "种草", "短视频", "直播话术", "广告语" }),
            ("图片生成", new[] { "图片", "prompt", "生成图", "美图" }),
            ("订单查询", new[] { "订单", "物流", "发货", "查单" }),
            // 未来其他意图...
        };

        public Task<string> ClassifyAsync(string userMessage, CancellationToken ct = default)
        {
            var msg = userMessage.ToLowerInvariant();

            foreach (var (intent, keywords) in _rules)
            {
                if (keywords.Any(k => msg.Contains(k.ToLowerInvariant())))
                    return Task.FromResult(intent);
            }

            return Task.FromResult("普通聊天");
        }
    }
}
