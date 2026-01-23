using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.AI;
using System.Text.Json;

namespace MyProject.Infrastructure.Services;

public class AiChatService : IAiChatService
{
    private readonly SemanticKernelConfig _skConfig;
    private readonly IRepository<Conversation> _conversationRepository;

    public AiChatService(
        SemanticKernelConfig skConfig,
        IRepository<Conversation> conversationRepository)
    {
        _skConfig = skConfig;
        _conversationRepository = conversationRepository;
    }

    public async Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid sellerId, SendChatMessageCommand command)
    {
        // 1. 获取或创建会话
        var conversation = await GetOrCreateConversation(sellerId, command.ConversationId);

        // 2. 添加用户消息
        var userMsg = ChatMessage.FromUser(command.Message);
        conversation.AddMessage(userMsg);

        // 3. Semantic Kernel 处理
        var kernel = _skConfig.Kernel;
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();

        // 系统 Prompt（固定指令，告诉模型如何处理意图）
        chatHistory.AddSystemMessage(@"
你是中国中小电商卖家的智能店小二，精通淘宝/拼多多/抖音运营。
用户输入一句话意图，你要：
- 如果包含'优化'、'详情'、'营销'、'图片'等词，判断为商品优化任务
- 自动完成优化，返回严格 JSON 格式（无多余文字）
- JSON 结构必须是：
{
  ""reply_text"": ""已完成优化，以下是结果"",
  ""type"": ""optimize_result"",
  ""data"": {
    ""optimized_title"": ""新标题"",
    ""optimized_description"": ""Markdown 格式描述"",
    ""marketing_plan"": {
      ""short_video_script"": ""短视频脚本"",
      ""planting_text"": ""种草文案"",
      ""live_script"": ""直播话术"",
      ""key_selling_points"": [""卖点1"", ""卖点2""]
    },
    ""image_prompts"": [""通义万相 prompt1"", ""prompt2""]
  }
}
- 如果不是优化意图，返回普通回复 JSON：{ ""reply_text"": ""..."", ""type"": ""text"" }
");

        // 加载最近历史（控制 token）
        foreach (var msg in conversation.Messages.OrderByDescending(m => m.Timestamp).Take(10).Reverse())
        {
            chatHistory.AddMessage(msg.IsFromUser ? AuthorRole.User : AuthorRole.Assistant, msg.Content);
        }

        // 当前输入
        chatHistory.AddUserMessage(command.Message);

        // 执行生成（强制 JSON 输出）
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.7,
            MaxTokens = 3000,
            ResponseFormat = "json_object"  // 关键：强制 JSON
        };

        var result = await chatService.GetChatMessageContentAsync(chatHistory, settings);
        var replyJson = result.Content ?? "{\"reply_text\":\"抱歉，无法处理\",\"type\":\"text\"}";

        // 4. 解析 JSON
        string replyText = "处理中...";
        string messageType = "text";
        object? structuredData = null;

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(replyJson);
            if (parsed != null)
            {
                replyText = parsed["reply_text"].GetString() ?? replyText;
                messageType = parsed["type"].GetString() ?? messageType;

                if (messageType == "optimize_result" && parsed.TryGetValue("data", out var dataElem))
                {
                    structuredData = JsonSerializer.Deserialize<OptimizeProductData>(dataElem.GetRawText());
                }
            }
        }
        catch (Exception ex)
        {
            replyText = $"AI 输出解析失败：{ex.Message}";
            Console.WriteLine($"JSON 解析错误: {ex.Message}\n原始: {replyJson}");
        }

        // 5. 添加 AI 回复
        var aiMsg = ChatMessage.FromAI(replyText, messageType, structuredData);
        conversation.AddMessage(aiMsg);

        await _conversationRepository.UpdateAsync(conversation);
        //await _conversationRepository.SaveChangesAsync();

        // 6. 返回 DTO
        return new ChatMessageReplyDto
        {
            ConversationId = conversation.Id,
            Messages = conversation.Messages.Select(m => new ChatMessageItemDto
            {
                IsFromUser = m.IsFromUser,
                Content = m.Content,
                MessageType = m.Type,
                Data = m.DataJson != null ? JsonSerializer.Deserialize<object>(m.DataJson) : null
            }).ToList()
        };
    }

    private async Task<Conversation> GetOrCreateConversation(Guid sellerId, Guid? conversationId)
    {
        Conversation? conversation = null;

        if (conversationId.HasValue)
        {
            conversation = await _conversationRepository.FirstOrDefaultAsync(
                c => c.Id == conversationId.Value && c.SellerId == sellerId);
        }

        if (conversation == null)
        {
            conversation = Conversation.Create(sellerId);
            await _conversationRepository.AddAsync(conversation);
            //await _conversationRepository.SaveChangesAsync();  // 立即保存，确保 ID 可用
        }

        return conversation;
    }
}