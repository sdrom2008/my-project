using Microsoft.EntityFrameworkCore;
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
using System.Text.Json;

namespace Synerixis.Infrastructure.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IIntentClassifier _intentClassifier;
        private readonly IGeneralChatAgent _generalChatAgent;          // 新增通用聊天 agent
        private readonly IAgentRouter _agentRouter;
        private readonly IConversationRepository _conversationRepo;   // 保存历史

        public AiChatService(
            IIntentClassifier intentClassifier,
            IGeneralChatAgent generalChatAgent,
            IAgentRouter agentRouter,
            IConversationRepository conversationRepo)
        {
            _intentClassifier = intentClassifier;
            _generalChatAgent = generalChatAgent;
            _agentRouter = agentRouter;
            _conversationRepo = conversationRepo;
        }

        public async Task<SynerixisResponse> HandleMessageAsync(string conversationId, string userInput, string sellerId)
        {
            var context = await _conversationRepo.GetContextAsync(conversationId, sellerId);
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
                await _conversationRepo.AppendMessagesAsync(conversationId ,sellerId, messages);

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

        public Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid userId, SendChatMessageCommand command)
        {
            throw new NotImplementedException();
        }
    }
}