using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Agents
{
    public class GeneralChatAgent : IGeneralChatAgent
    {
        private readonly IChatCompletionService _chatService;

        public GeneralChatAgent(IChatCompletionService chatService)
        {
            _chatService = chatService;
        }

        public async Task<string> GenerateReplyAsync(string input, ChatContext context)
        {
            // 简单实现，先让它编译通过
            var historyText = string.Join("\n", context.Messages.TakeLast(5)
                .Select(m => $"{(m.IsFromUser ? "用户" : "AI")}: {m.Content}"));

            var prompt = $"""
你是 Synerixis 的智能小二，友好亲切，用口语回复。
历史：{historyText}
用户：{input}
直接回复，不要解释。
""";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("你是友好客服");
            chatHistory.AddUserMessage(prompt);

            var settings = new OpenAIPromptExecutionSettings { Temperature = 0.8, MaxTokens = 150 };

            var response = await _chatService.GetChatMessageContentAsync(chatHistory, settings);
            return response.Content ?? "抱歉，我没听清～再说一次？";
        }
    }
}
