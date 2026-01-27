// MyProject.Infrastructure/Services/AgentRouter.cs
using Json.Schema;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Enums;
using static Synerixis.Application.Interfaces.IAgent;

namespace Synerixis.Infrastructure.Services
{
    public class AgentRouter : IAgentRouter
    {
        private readonly Dictionary<ChatIntent, IAgent> _agents;

        public AgentRouter(IEnumerable<IAgent> agents)
        {
            _agents = agents
                .GroupBy(a => a.SupportedIntent)
                .ToDictionary(
                    g => g.Key,
                    g => g.First()  // 如果有重复意图，取第一个
                );
        }

        public IAgent? GetAgent(ChatIntent intent)
        {
            _agents.TryGetValue(intent, out var agent);
            return agent;
        }

        public async Task<AgentProcessResult> RouteAsync(ChatIntent intent, string userInput, ChatContext context)
        {
            var agent = GetAgent(intent);
            if (agent == null)
            {
                return new AgentProcessResult(
                    Messages: new List<ChatMessageDto>
                    {
                        new ChatMessageDto
                        {
                            IsFromUser = false,
                            Content = "抱歉，暂不支持该功能～",
                            MessageType = "text",
                            Timestamp = DateTime.UtcNow
                        }
                    },
                    Success: false,
                    ErrorMessage: $"未知意图: {intent}"
                );
            }

            return await agent.ProcessAsync(userInput, context);
        }
    }
}