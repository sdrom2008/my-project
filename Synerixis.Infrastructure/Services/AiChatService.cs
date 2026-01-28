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
        private readonly IRepository<Seller> _sellerRepository;

        public AiChatService(IIntentClassifier intentClassifier,IAgentRouter agentRouter,IGeneralChatAgent generalChatAgent,IConversationRepository conversationRepository,ILogger<AiChatService> logger , IRepository<Seller> sellerRepository)
        {
            _intentClassifier = intentClassifier;
            _agentRouter = agentRouter;
            _generalChatAgent = generalChatAgent;
            _conversationRepository = conversationRepository;
            _logger = logger;
            _sellerRepository = sellerRepository;
        }

        public async Task<SynerixisResponse> HandleMessageAsync(string conversationId, string userInput, string sellerId)
        {
            var context = await _conversationRepository.GetContextAsync(conversationId, sellerId);
            var messages = new List<ChatMessageDto>{new() { IsFromUser = true, Content = userInput, MessageType = "text" }};

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

                // 保存 顺带获取conid
                conversationId = (await _conversationRepository.AppendMessagesAsync(conversationId ,sellerId, messages)).ToString();

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
            // 先查 seller（用仓储）
            var seller = await _sellerRepository.FirstOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null) throw new UnauthorizedAccessException("商户不存在");

            // 校验额度或订阅
            if (seller.FreeQuota <= 0 && (seller.SubscriptionEnd == null || seller.SubscriptionEnd < DateTime.UtcNow))
            {
                throw new InvalidOperationException("免费额度已用完，请续费");
            }

            // 消耗额度
            seller.ConsumeQuota();  // 调用实体方法（你之前加的）
            await _sellerRepository.SaveChangesAsync();  // 用仓储保存

            _logger.LogInformation("ProcessUserMessageAsync - SellerId: {SellerId}, ConvId: {ConvId}, Msg: {Msg}", sellerId, conversationId, message);

            // 1. 处理 conversationId：如果 null，创建新会话
            Guid actualConvId = conversationId ?? Guid.Empty;  // 直接用前端传的值

            // 如果是新建，提前创建并保存 Conversation
            if (actualConvId == Guid.Empty)
            {
                var newConv = Conversation.Create(sellerId, "新对话 - " + DateTime.UtcNow.ToString("MM-dd HH:mm"));
                await _conversationRepository.AddAsync(newConv);
                await _conversationRepository.SaveChangesAsync();  // 立即保存！确保 Id 写入 DB
                actualConvId = newConv.Id;
                _logger.LogInformation("新建并保存 Conversation ID: {ConvId}", actualConvId);
            }

            // 获取上下文（如果 null 或 Empty，Repository 会处理新建）
            var context = await _conversationRepository.GetContextAsync(actualConvId.ToString(), sellerId.ToString());

            // 构建用户消息
            var userMsg = new ChatMessageDto
            {
                IsFromUser = true,
                Content = message,
                MessageType = "text",
                Data = extraData,
                Timestamp = DateTime.UtcNow
            };

            // 4. 意图分类
            var intent = await _intentClassifier.ClassifyAsync(message, context.Messages.AsReadOnly());

            _logger.LogDebug("用户消息: {Message}, 意图: {Intent}", message, intent);

            // 5. 根据意图处理
            List<ChatMessageDto> replyMsgs;

            if (intent == ChatIntent.GeneralChat || intent == ChatIntent.Unknown)
            {
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
                var agent = _agentRouter.GetAgent(intent);
                if (agent == null)
                {
                    replyMsgs = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    IsFromUser = false,
                    Content = "抱歉，暂不支持该功能～",
                    MessageType = "text",
                    Timestamp = DateTime.UtcNow
                }
            };
                }
                else
                {
                    var agentResult = await agent.ProcessAsync(message, context);
                    replyMsgs = agentResult.Success ? agentResult.Messages.ToList() : new List<ChatMessageDto>
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
            }

            // 6. 保存所有消息（用户 + AI 回复）
            var allMsgs = new List<ChatMessageDto> { userMsg };
            allMsgs.AddRange(replyMsgs);

            //获取实际的 ConversationId（如果是新建会话，Repository 会返回新的 Id）
            actualConvId = await _conversationRepository.AppendMessagesAsync(actualConvId.ToString(),sellerId.ToString(),allMsgs);

            // 返回时用实际的 ConversationId（Repository 会更新 actualConvId 如果新建）
            return new ChatMessageReplyDto
            {
                ConversationId = actualConvId,  // 如果新建，Repository 会返回真实 Id（需调整）
                Messages = allMsgs
            };
        }
    }
}