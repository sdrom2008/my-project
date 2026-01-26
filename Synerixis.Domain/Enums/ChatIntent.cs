using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Enums
{
    public enum ChatIntent
    {
        Unknown = 0,
        GeneralChat = 1,          // 普通闲聊
        OrderQuery = 2,
        ProductOptimization = 3,
        Appointment = 4,
        AfterSale = 5,
        MarketingFollowup = 6
    }

    public record ChatMessage(
    bool IsFromUser,
    string Content,
    string MessageType = "text",
    object? Data = null,
    DateTime Timestamp = default)
    {
        public ChatMessage() : this(false, string.Empty) { }
    }
}
