using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class SendChatMessageCommand
    {
        public Guid? ConversationId { get; set; }          // 可选，如果不传则新建会话
        public string Message { get; set; } = string.Empty; // 用户输入的消息
        public Dictionary<string, string>? ExtraData { get; set; } // 扩展：商品标题、描述、类目等
    }
}
