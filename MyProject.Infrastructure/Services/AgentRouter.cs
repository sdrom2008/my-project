// MyProject.Infrastructure/Services/AgentRouter.cs
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;

namespace MyProject.Infrastructure.Services
{
    public class AgentRouter
    {
        private readonly Dictionary<string, IAgent> _agents;

        public AgentRouter(IEnumerable<IAgent> agents)
        {
            _agents = agents.ToDictionary(a => a.Name, a => a);
        }

        public async Task<AgentResponse> RouteAsync(AgentContext context, string intent)
        {
            if (_agents.TryGetValue(intent, out var agent))
            {
                return await agent.ExecuteAsync(context);
            }

            // 默认兜底
            return new AgentResponse
            {
                replyText = "抱歉，暂不支持该功能～",
                messageType = "text",
                success = false,
                errorMessage = "未知意图: " + intent
            };
        }
    }
}