using Microsoft.Extensions.AI;
using MyProject.Application.DTOs;
using System.Threading.Tasks;

namespace MyProject.Application.Interfaces
{
    /// <summary>
    /// AI 聊天服务接口
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// 聊天交互
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        //Task<ChatResponse> ChatAsync(Guid userId, ChatRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid userId, SendChatMessageCommand command);

        
    }
}