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
        public ConversationRepository(AppDbContext context) : base(context) {
        }

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

            if (!messages.Any())
                throw new ArgumentException("没有消息可保存");

            var conv = await _dbSet
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == convGuid && c.SellerId == sellerGuid);

            await _context.SaveChangesAsync();

            if (conv == null)
            {
                var firstMsg = messages.First();
                //conv = Conversation.Create(sellerGuid);
                _dbSet.Add(conv);

                //新建会话后 立即保存
                await _context.SaveChangesAsync();
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
                // 可加日志：_logger.LogInformation("保存消息成功 - 会话ID: {ConvId}, 消息数: {Count}", conv.Id, messages.Count());
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
                    throw new InvalidOperationException("会话已被删除，请新建会话");
                }

                // 覆盖重试
                entry.OriginalValues.SetValues(databaseValues);
                entry.CurrentValues.SetValues(databaseValues);

                try
                {
                    await _context.SaveChangesAsync();
                    // _logger.LogWarning("并发冲突已自动解决 - 会话ID: {ConvId}", conv.Id);
                }
                catch (Exception retryEx)
                {
                    throw new InvalidOperationException($"保存失败，已重试并发冲突: {retryEx.Message}", retryEx);
                }
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"数据库更新失败: {ex.InnerException?.Message ?? ex.Message}", ex);
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
