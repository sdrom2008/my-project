using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Application.Services;
using Synerixis.Domain.Entities;
using Synerixis.Domain.Enums;
using Synerixis.Infrastructure.AI;
using Synerixis.Infrastructure.Data;
using Synerixis.Infrastructure.Repositories;
using System.Text.Json;

namespace Synerixis.Infrastructure.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IIntentClassifier _intentClassifier;
        private readonly IGeneralChatAgent _generalChatAgent;          // 新增通用聊天 agent
        private readonly IAgentRouter _agentRouter;
        private readonly IConversationRepository _conversationRepository;   // 保存历史
        private readonly ILogger<AiChatService> _logger;

        public AiChatService(IIntentClassifier intentClassifier,IAgentRouter agentRouter,IGeneralChatAgent generalChatAgent,IConversationRepository conversationRepository,ILogger<AiChatService> logger)
        {
            _intentClassifier = intentClassifier;
            _agentRouter = agentRouter;
            _generalChatAgent = generalChatAgent;
            _conversationRepository = conversationRepository;
            _logger = logger;
        }

        public async Task<SynerixisResponse> HandleMessageAsync(string conversationId, string userInput, string sellerId)
        {
            var context = await _conversationRepository.GetContextAsync(conversationId, sellerId);
            var messages = new List<ChatMessageDto>
        {
            new() { IsFromUser = true, Content = userInput, MessageType = "text" }
        };

            try
            {
                var intent = await _intentClassifier.ClassifyAsync(userInput, context.Messages);

                ChatMessageDto aiMessage;

                if (intent == ChatIntent.GeneralChat || intent == ChatIntent.Unknown)
                {
                    // 默认自然聊天
                    var reply = await _generalChatAgent.GenerateReplyAsync(userInput, context);
                    aiMessage = new ChatMessageDto
                    {
                        IsFromUser = false,
                        Content = reply,
                        MessageType = "text"
                    };
                }
                else
                {
                    // 路由到专业 Agent
                    var agent = _agentRouter.GetAgent(intent);
                    var result = await agent.ProcessAsync(userInput, context);

                    // 假设 agent 返回结构化消息列表
                    messages.AddRange(result.Messages);
                    return new SynerixisResponse
                    {
                        ConversationId = conversationId,
                        Messages = messages,
                        Success = true
                    };
                }

                messages.Add(aiMessage);

                // 保存
                await _conversationRepository.AppendMessagesAsync(conversationId ,sellerId, messages);

                return new SynerixisResponse
                {
                    ConversationId = conversationId,
                    Messages = messages,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                messages.Add(new ChatMessageDto
                {
                    IsFromUser = false,
                    Content = "抱歉，刚才出了点小状况～请再试一次哦！",
                    MessageType = "text"
                });

                return new SynerixisResponse
                {
                    ConversationId = conversationId,
                    Messages = messages,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public Task<SynerixisResponse> HandleMessageAsync(string conversationId, string userInput, string sellerId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid sellerId,Guid? conversationId,string message,Dictionary<string, string>? extraData = null)
        {
            // 1. 处理 conversationId：如果 null，创建新会话
            Guid actualConvId;

            if (!conversationId.HasValue)
            {
                var newConv = Conversation.Create(sellerId, "新对话 - " + DateTime.UtcNow.ToString("MM-dd HH:mm"));
                await _conversationRepository.AddAsync(newConv);
                await _conversationRepository.SaveChangesAsync();  // 假设 Repository 有 SaveChangesAsync 方法，或用 _context.SaveChangesAsync()
                actualConvId = newConv.Id;
                _logger.LogInformation("创建新会话 ID: {ConvId} for Seller: {SellerId}", actualConvId, sellerId);
            }
            else
            {
                actualConvId = conversationId.Value;
            }

            // 2. 获取当前上下文
            var context = await _conversationRepository.GetContextAsync(actualConvId.ToString(), sellerId.ToString());

            // 3. 构建用户消息 DTO
            var userMsg = new ChatMessageDto
            {
                IsFromUser = true,
                Content = message,
                MessageType = "text",
                Data = extraData,  // 如果 extraData 是结构化数据，直接存入 Data
                Timestamp = DateTime.UtcNow
            };

            // 4. 意图分类
            var intent = await _intentClassifier.ClassifyAsync(message, context.Messages.AsReadOnly());

            _logger.LogDebug("用户消息: {Message}, 意图: {Intent}", message, intent);

            // 5. 根据意图处理
            List<ChatMessageDto> replyMsgs;

            if (intent == ChatIntent.GeneralChat || intent == ChatIntent.Unknown)
            {
                // 默认自然聊天
                var aiReplyText = await _generalChatAgent.GenerateReplyAsync(message, context);
                replyMsgs = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    IsFromUser = false,
                    Content = aiReplyText,
                    MessageType = "text",
                    Timestamp = DateTime.UtcNow
                }
            };
            }
            else
            {
                // 路由到专业 Agent
                var agent = _agentRouter.GetAgent(intent);

                if (agent == null)
                {
                    _logger.LogWarning("未找到 Agent for Intent: {Intent}", intent);
                    replyMsgs = new List<ChatMessageDto>
                {
                    new ChatMessageDto
                    {
                        IsFromUser = false,
                        Content = "抱歉，暂不支持该功能～请描述得更详细些！",
                        MessageType = "text",
                        Timestamp = DateTime.UtcNow
                    }
                };
                }
                else
                {
                    var agentResult = await agent.ProcessAsync(message, context);
                    if (!agentResult.Success)
                    {
                        _logger.LogError("Agent 处理失败: {Error}", agentResult.ErrorMessage);
                        replyMsgs = new List<ChatMessageDto>
                    {
                        new ChatMessageDto
                        {
                            IsFromUser = false,
                            Content = "处理出错啦～请稍后再试！",
                            MessageType = "text",
                            Timestamp = DateTime.UtcNow
                        }
                    };
                    }
                    else
                    {
                        replyMsgs = agentResult.Messages.ToList();
                    }
                }
            }

            // 6. 保存所有消息（用户 + AI 回复）
            var allMsgs = new List<ChatMessageDto> { userMsg };
            allMsgs.AddRange(replyMsgs);

            await _conversationRepository.AppendMessagesAsync(
                actualConvId.ToString(),
                sellerId.ToString(),
                allMsgs);

            // 7. 返回给前端
            return new ChatMessageReplyDto
            {
                ConversationId = actualConvId,  // Guid 类型
                Messages = allMsgs
            };
        }
    }
}