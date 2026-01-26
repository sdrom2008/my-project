using Synerixis.Application.DTOs;
using Synerixis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IAgent
    {
        string Name { get; }
        string Description { get; }

        ChatIntent SupportedIntent { get; }
        Task<AgentProcessResult> ProcessAsync(string userInput, ChatContext context);
        public record AgentProcessResult(IReadOnlyList<ChatMessageDto> Messages, bool Success = true,string? ErrorMessage = null);        // 未来可加：bool NeedHumanIntervention, string SuggestedAction 等

        Task<AgentResponse> ExecuteAsync(AgentContext context, CancellationToken ct = default);
    }

}
