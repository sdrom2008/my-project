using Synerixis.Application.DTOs;
using Synerixis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IConversationRepository : IRepository<Conversation>
    {
        Task<Conversation?> GetWithMessagesAsync(Guid id);

        //新曾方法，用于获取聊天上下文
        Task<ChatContext> GetContextAsync(string conversationId, string sellerId); 
        Task AppendMessagesAsync(string conversationId, string sellerId, IEnumerable<ChatMessageDto> messages);
    }
}
