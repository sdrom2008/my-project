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
        ChatIntent SupportedIntent { get; }
        Task<AgentProcessResult> ProcessAsync(string userInput, ChatContext context);
    }

    public record AgentProcessResult(
    IReadOnlyList<ChatMessageDto> Messages,
    bool Success = true,
    string? ErrorMessage = null);
}
