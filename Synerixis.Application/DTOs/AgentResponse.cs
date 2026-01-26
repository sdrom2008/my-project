using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    // 后端 DTO（返回给前端）
    public class AgentResponse
    {
        /// <summary>
        /// 会话ID（字符串格式的 Guid，前端直接用）
        /// </summary>
        public string conversationId { get; set; } = string.Empty;

        /// <summary>
        /// 给用户看的回复文本（普通聊天或兜底文案）
        /// </summary>
        public string replyText { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型（前端据此决定渲染方式）
        /// 常见值：text / optimize_result / marketing_copy / image_prompt / order_query / error
        /// </summary>
        public string messageType { get; set; } = "text";

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; set; } = true;

        /// <summary>
        /// 错误信息（success=false 时有值）
        /// </summary>
        public string? errorMessage { get; set; }

        /// <summary>
        /// 结构化数据（根据 messageType 不同，内容不同）
        /// 前端根据 messageType 动态解析这个对象
        /// </summary>
        public object? data { get; set; }
    }
}
