using System.Threading.Tasks;

namespace MyProject.Application.Services;

public interface IAiChatService
{
    Task<string> ProcessUserMessageAsync(string userMessage, string? conversationId = null);
}