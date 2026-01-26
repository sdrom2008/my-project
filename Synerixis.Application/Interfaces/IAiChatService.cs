using Microsoft.Extensions.AI;
using Synerixis.Application.DTOs;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    /// <summary>
    /// AI 聊天服务接口
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid sellerId,
            Guid? conversationId,
            string message,
            Dictionary<string, string>? extraData = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="userInput"></param>
        /// <param name="sellerId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<SynerixisResponse> HandleMessageAsync(string conversationId,string userInput,string sellerId,CancellationToken ct = default);
    }
}