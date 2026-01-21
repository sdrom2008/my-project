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
        /// 发送消息并获取 AI 回复
        /// </summary>
        /// <param name="conversationId">会话 ID，用于保持上下文</param>
        /// <param name="message">用户输入的消息</param>
        /// <returns>AI 回复的结构化结果</returns>
        Task<AiChatReply> ChatAsync(string conversationId, string message);
    }
}