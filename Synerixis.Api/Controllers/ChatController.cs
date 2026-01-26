using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synerixis.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Synerixis.Api.Extensions;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Entities;

namespace Synerixis.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]  // 所有接口都需要登录（Seller 身份）
    public class ChatController : BaseApiController
    {
        private readonly IAiChatService _aiChatService;
        private readonly IRepository<Conversation> _conversationRepository;

        public ChatController(
            IAiChatService aiChatService,
            IRepository<Conversation> conversationRepository)
        {
            _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        }

        /// <summary>
        /// 发送聊天消息（支持商品优化、营销方案、智能客服等意图）
        /// </summary>
        /// <param name="command">聊天输入（消息内容 + 可选会话ID）</param>
        /// <returns>聊天回复（包含结构化结果，如商品优化数据）</returns>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ChatMessageReplyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageCommand command)
        {
            // 1. 参数校验
            if (command == null || string.IsNullOrWhiteSpace(command.Message))
            {
                return BadRequest(new { message = "消息内容不能为空" });
            }

            // 2. 从 JWT 获取当前商家 ID（使用扩展方法）
            Guid sellerId;
            try
            {
                sellerId = User.GetSellerId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("无效的商家身份");
            }

            // 3. 调用 Application 层服务处理（核心业务逻辑）
            var reply = await _aiChatService.ProcessUserMessageAsync(sellerId, command);

            // 4. 返回结构化结果
            return Ok(reply);
        }

        /// <summary>
        /// 获取当前商家的所有会话列表
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            Guid sellerId;
            try
            {
                sellerId = User.GetSellerId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("无效的商家身份");
            }

            var conversations = await _conversationRepository.GetAllAsync(
                c => c.SellerId == sellerId,
                orderBy: q => q.OrderByDescending(c => c.LastActiveAt));

            var result = conversations.Select(c => new
            {
                id = c.Id.ToString(),
                title = c.Title,
                lastMessage = c.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Content ?? "暂无消息",
                lastActiveAt = c.LastActiveAt?.ToString("yyyy-MM-dd HH:mm")
            });

            return Ok(result);
        }

        /// <summary>
        /// 获取指定会话的完整历史消息（用于前端加载历史聊天记录）
        /// </summary>
        /// <param name="id">会话ID</param>
        [HttpGet("conversation/{id}")]
        public async Task<IActionResult> GetConversationMessages(Guid id)
        {
            Guid sellerId;
            try
            {
                sellerId = User.GetSellerId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("无效的商家身份");
            }

            var conversation = await _conversationRepository.FirstOrDefaultAsync(
                c => c.Id == id && c.SellerId == sellerId,
                include: q => q.Include(c => c.Messages));

            if (conversation == null)
            {
                return NotFound(new { message = "会话不存在" });
            }

            var messages = conversation.Messages
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    isFromUser = m.IsFromUser,
                    content = m.Content,
                    messageType = m.Type,
                    data = m.DataJson != null
                        ? JsonSerializer.Deserialize<object>(m.DataJson)
                        : null
                });

            return Ok(new
            {
                conversationId = conversation.Id.ToString(),
                title = conversation.Title,
                messages
            });
        }
    }
}