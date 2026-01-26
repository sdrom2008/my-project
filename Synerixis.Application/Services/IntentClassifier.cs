// Synerixis.Application/Services/LlmIntentClassifier.cs
// 或直接叫 IntentClassifier.cs （如果你决定覆盖旧版）

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Synerixis.Application.DTOs;
using Synerixis.Application.Interfaces;
using Synerixis.Domain.Enums;

namespace Synerixis.Application.Services;

public class IntentClassifier : IIntentClassifier
{
    private readonly IChatCompletionService _chatService;

    public IntentClassifier(IChatCompletionService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userInput"></param>
    /// <param name="recentHistory"></param>
    /// <returns></returns>
    public async Task<ChatIntent> ClassifyAsync(string userInput,IReadOnlyList<ChatMessageDto> recentHistory)
    {
        var historyText = string.Join("\n", recentHistory
            .TakeLast(6)
            .Select(m => $"{(m.IsFromUser ? "用户" : "AI")}: {m.Content.Trim()}"));

        var systemPrompt = """
你是一个精准的意图分类器。
只输出以下其中一个类别英文名称，不要输出任何其他文字、解释、标点或换行。

可用类别（严格只能选其中之一）：
GeneralChat          - 普通闲聊、问候、天气、表情、夸赞、无明确业务需求
OrderQuery           - 查询订单、物流状态、发货时间、到货时间、订单详情
ProductOptimization  - 商品标题/描述/主图/详情页优化、文案建议、图片分析
Appointment          - 预约时间、咨询档期、空位查询、安排见面
AfterSale            - 退款、退货、换货、投诉、售后服务问题
MarketingFollowup    - 复购引导、商品推荐、催评价、感谢、促销活动相关

""";

        var userMessage = $"""
用户最新消息：{userInput}

最近几轮对话：
{historyText}

现在分类，只输出类别名称，例如：GeneralChat
""";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(userMessage);

        try
        {
            // 在方法里改成这样：
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.1,
                MaxTokens = 20,
                TopP = 0.1
            };

            var result = await _chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: executionSettings  // 注意用命名参数
            );

            var category = result.Content?.Trim()?.ToLowerInvariant() ?? "unknown";

            return category switch
            {
                "generalchat" => ChatIntent.GeneralChat,
                "orderquery" => ChatIntent.OrderQuery,
                "productoptimization" => ChatIntent.ProductOptimization,
                "appointment" => ChatIntent.Appointment,
                "aftersale" => ChatIntent.AfterSale,
                "marketingfollowup" => ChatIntent.MarketingFollowup,
                _ => ChatIntent.Unknown
            };
        }
        catch (Exception ex)
        {
            // 可以记录日志
            //_logger.LogWarning("意图分类失败，使用默认 Unknown: {Error}", ex.Message);
            return ChatIntent.Unknown;
        }
    }
}