using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyProject.Application.DTOs; // 确保已引用 ChatResponse 类型
using MyProject.Application.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Infrastructure.Services;

public class AiChatService : IAiChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiChatService> _logger;
    private readonly HttpClient _httpClient;

    // 内存存储会话历史
    private static readonly Dictionary<string, List<object>> _history = new();

    public AiChatService(
        IConfiguration configuration,
        ILogger<AiChatService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<AiChatReply> ChatAsync(string conversationId, string message)
    {
        if (string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("ConversationId 或 Message 不能为空");

        var apiKey = _configuration["DashScope:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("未配置 DashScope ApiKey");

        // 获取或初始化历史
        if (!_history.TryGetValue(conversationId, out var messages))
        {
            messages = new List<object>();
            _history[conversationId] = messages;
        }

        messages.Add(new { role = "user", content = message });

        try
        {
            var requestBody = new
            {
                model = "qwen-max",
                input = new { messages },
                parameters = new
                {
                    // 强提示（之前跑通的版本）
                    prompt = @"你是一个专业的智能客服助手。请严格按照以下 JSON 格式回复，不要添加任何额外文字、解释、markdown 或换行：
{
  ""reply"": ""给用户的自然语言友好回复"",
  ""type"": ""text|order|appointment|product|other"",
  ""data"": {} // 根据 type 可选填充数据，如订单号、状态等
}"
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation");
            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
            httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(httpRequest);
            var responseText = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("通义千问调用失败 {StatusCode}: {Response}", httpResponse.StatusCode, responseText);
                return new AiChatReply { Reply = "抱歉，AI 服务暂时不可用" };
            }

            var aiJson = JObject.Parse(responseText);
            var aiText = aiJson["output"]?["text"]?.ToString() ?? "";

            // 解析 JSON
            JObject parsed;
            try
            {
                parsed = JObject.Parse(aiText);
            }
            catch
            {
                var start = aiText.IndexOf('{');
                var end = aiText.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    var jsonPart = aiText.Substring(start, end - start + 1);
                    parsed = JObject.Parse(jsonPart);
                }
                else
                {
                    parsed = new JObject { ["reply"] = aiText, ["type"] = "text" };
                }
            }

            var reply = parsed["reply"]?.ToString() ?? aiText;
            var type = parsed["type"]?.ToString() ?? "text";
            var data = parsed["data"] as JObject;

            messages.Add(new { role = "assistant", content = reply });

            return new AiChatReply
            {
                Reply = reply,
                Type = type,
                Data = data?.ToObject<Dictionary<string, object>>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI 聊天异常");
            return new AiChatReply { Reply = "服务器内部错误，请稍后再试" };
        }
    }
}