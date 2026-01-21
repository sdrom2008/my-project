using AlibabaCloud.SDK.Dashscope20230320;
using AlibabaCloud.SDK.Dashscope20230320.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyProject.Application.Interfaces;
using MyProject.Application.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyProject.Infrastructure.Services;

public class AiChatService : IAiChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiChatService> _logger;

    // 内存存储会话历史（测试用，生产换 Redis 或数据库）
    private static readonly Dictionary<string, List<object>> _conversationHistory = new();

    public AiChatService(IConfiguration configuration, ILogger<AiChatService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatResponse> ChatAsync(string conversationId, string message)
    {
        if (string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("ConversationId 和 Message 不能为空");
        }

        var apiKey = _configuration["DashScope:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("未配置 DashScope API Key");
        }

        // 获取或初始化会话历史
        if (!_conversationHistory.TryGetValue(conversationId, out var history))
        {
            history = new List<object>();
            _conversationHistory[conversationId] = history;
        }

        // 添加用户消息到历史
        history.Add(new { role = "user", content = message });

        try
        {
            var client = new Client(new AlibabaCloud.OpenApiClient.Config
            {
                AccessKeyId = apiKey,
                AccessKeySecret = apiKey,  // DashScope 用 ApiKey 作为 Secret
                Endpoint = "dashscope.aliyuncs.com",
                RegionId = "cn-hangzhou"
            });

            var request = new RunGenerationRequest
            {
                Model = "qwen-max",  // 或 qwen-turbo 等
                Input = new RunGenerationRequestInput
                {
                    Messages = history,
                    // 强提示：要求输出严格 JSON 格式
                    Prompt = @"你是一个专业的智能客服助手。请严格按照以下 JSON 格式回复，不要添加任何额外文字、解释或 markdown：
{
  ""reply"": ""给用户的自然语言友好回复"",
  ""type"": ""text|order|appointment|product"",
  ""data"": { ""key"": ""value"" }  // 根据 type 填充相关数据，可为空对象
}"
                }
            };

            var response = await client.RunGenerationAsync(request);

            if (response.StatusCode != 200 || response.Body?.Output?.Text == null)
            {
                _logger.LogError("通义千问调用失败: {Code} {Message}", response.StatusCode, response.Body?.Message);
                return new ChatResponse { Reply = "抱歉，AI 服务暂时不可用，请稍后再试", Type = "text" };
            }

            var aiRawText = response.Body.Output.Text;

            // 尝试解析 JSON
            JObject jsonReply;
            try
            {
                jsonReply = JObject.Parse(aiRawText);
            }
            catch (JsonException)
            {
                // 如果不是纯 JSON，尝试提取 JSON 块
                var jsonStart = aiRawText.IndexOf('{');
                var jsonEnd = aiRawText.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonStr = aiRawText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    jsonReply = JObject.Parse(jsonStr);
                }
                else
                {
                    // 回退到纯文本
                    jsonReply = new JObject
                    {
                        ["reply"] = aiRawText,
                        ["type"] = "text"
                    };
                }
            }

            var replyText = jsonReply["reply"]?.ToString() ?? aiRawText;
            var type = jsonReply["type"]?.ToString() ?? "text";
            var data = jsonReply["data"] as JObject;

            // 添加 AI 回复到历史（保持上下文连续）
            history.Add(new { role = "assistant", content = replyText });

            // 返回结构化结果
            return new ChatResponse
            {
                Reply = replyText,
                Type = type,
                Data = data?.ToObject<Dictionary<string, object>>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI 聊天处理异常，ConversationId: {Id}", conversationId);
            return new ChatResponse { Reply = "抱歉，服务器内部错误，请稍后再试", Type = "text" };
        }
    }
}

// 输出 DTO
public class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public Dictionary<string, object>? Data { get; set; }
}

// 输入 DTO（Controller 使用）
public class ChatRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}