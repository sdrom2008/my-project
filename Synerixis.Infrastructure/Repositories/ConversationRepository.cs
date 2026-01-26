using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Entities;
using Synerixis.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Infrastructure.Repositories
{
    public class ConversationRepository : Repository<Conversation>, IConversationRepository
    {
        public ConversationRepository(AppDbContext context) : base(context) { }

        public async Task<Conversation?> GetWithMessagesAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AppendMessagesAsync(string conversationId, string sellerId, IEnumerable<ChatMessageDto> messages)
        {
            if (!Guid.TryParse(conversationId, out var convGuid))
                throw new ArgumentException("无效的 conversationId");

            var conv = await _dbSet
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == convGuid);

            if (conv == null)
            {
                if (!Guid.TryParse(sellerId, out var sellerGuid))
                    throw new ArgumentException("无效的 sellerId");

                var firstMsg = messages.FirstOrDefault();
                if (firstMsg == null)
                    throw new InvalidOperationException("没有消息，无法创建会话");

                conv = Conversation.Create(sellerGuid);
                _dbSet.Add(conv);
            }

            foreach (var dto in messages)
            {
                // 用静态工厂创建 ChatMessage（避免直接 new 和 private setter）
                ChatMessage msg;

                if (dto.IsFromUser)
                {
                    msg = ChatMessage.FromUser(dto.Content);
                }
                else
                {
                    // AI 消息，带 type 和 data
                    object? dataObj = dto.Data;
                    msg = ChatMessage.FromAI(
                        content: dto.Content,
                        messageType: dto.MessageType,
                        data: dataObj
                    );
                }

                // Timestamp 如果前端传了就用，否则用现在
                if (dto.Timestamp != default)
                {
                    // 由于 Timestamp 是 private set，只能反射或改实体加 public setter（不推荐）
                    // 推荐方案：让实体 Timestamp 支持从工厂传入（下面改实体）
                    // 临时 workaround：不设置，让实体用默认值
                }

                conv.AddMessage(msg);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ChatContext> GetContextAsync(string conversationId, string sellerId)
        {
            if (!Guid.TryParse(conversationId, out var convGuid))
                throw new ArgumentException("无效的 conversationId");

            if (!Guid.TryParse(sellerId, out var sellerGuid))
                throw new ArgumentException("无效的 sellerId");

            var conv = await _dbSet
                .AsNoTracking()
                .Include(c => c.Messages.OrderBy(m => m.Timestamp))
                .FirstOrDefaultAsync(c => c.Id == convGuid && c.SellerId == sellerGuid);

            if (conv == null)
            {
                return new ChatContext
                {
                    ConversationId = conversationId,
                    SellerId = sellerId,
                    Messages = new List<ChatMessageDto>()
                };
            }

            var messages = conv.Messages.Select(m => new ChatMessageDto
            {
                IsFromUser = m.IsFromUser,
                Content = m.Content,
                MessageType = m.MessageType,
                Data = string.IsNullOrEmpty(m.DataJson) ? null : JsonConvert.DeserializeObject(m.DataJson), // 用 Newtonsoft
                Timestamp = m.Timestamp
            }).ToList();

            return new ChatContext
            {
                ConversationId = conv.Id.ToString(),
                SellerId = conv.SellerId.ToString(),
                Messages = messages
            };
        }
    }
}
