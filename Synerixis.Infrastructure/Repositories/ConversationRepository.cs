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
            if (!Guid.TryParse(sellerId, out var sellerGuid))
                throw new ArgumentException("无效的 sellerId");

            var conv = await _dbSet
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == convGuid && c.SellerId == sellerGuid);

            if (conv == null)
            {
                var firstMsg = messages.FirstOrDefault();
                if (firstMsg == null)
                    throw new InvalidOperationException("没有消息，无法创建会话");

                conv = Conversation.Create(sellerGuid);
                _dbSet.Add(conv);
            }

            foreach (var dto in messages)
            {
                ChatMessage msg;

                if (dto.IsFromUser)
                {
                    msg = ChatMessage.FromUser(dto.Content);
                }
                else
                {
                    object? dataObj = dto.Data;
                    msg = ChatMessage.FromAI(
                        content: dto.Content,
                        messageType: dto.MessageType,
                        data: dataObj
                    );
                }
                conv.Messages.Add(msg);
            }

            try
            {
                await _context.SaveChangesAsync();
                return;  // 成功，退出
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.SingleOrDefault();
                if (entry == null)
                {
                    throw new InvalidOperationException("并发异常无实体信息");
                }

                var databaseValues = entry.GetDatabaseValues();

                if (databaseValues == null)
                {
                    // 记录已被删除
                    throw new InvalidOperationException("会话已被删除，请新建会话");
                }

                // 重新加载客户端值（覆盖数据库变化，或合并）
                entry.OriginalValues.SetValues(databaseValues);
                entry.CurrentValues.SetValues(databaseValues);  // 简单覆盖：以当前实体为准

                // 如果需要合并自定义逻辑：读取 databaseValues，合并 Messages 等
                // 示例：var dbConv = (Conversation)databaseValues.ToObject();
                // 然后合并 Messages...
            }

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
