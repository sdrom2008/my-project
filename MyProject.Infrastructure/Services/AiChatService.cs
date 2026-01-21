using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyProject.Application.Services;
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

    // 会话历史存储（内存版，生产可换 Redis）
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

    public async Task<ChatResponse> ChatAsync(string conversationId, string userMessage)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("ConversationId 不能为空");

        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("Message 不能为空");

        var apiKey = _configuration["DashScope:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("未配置 DashScope ApiKey");

        // 获取或初始化会话历史
        if (!_history.TryGetValue(conversationId, out var messages))
        {
            messages = new List<object>();
            _history[conversationId] = messages;
        }

        // 添加用户消息
        messages.Add(new { role = "user", content = userMessage });

        try
        {
            var requestBody = new
            {
                model = "qwen-max",
                input = new
                {
                    messages
                },
                parameters = new
                {
                    // 强提示：必须输出纯 JSON，不要多余文字
                    prompt = @"请严格按照以下 JSON 格式回复，不要添加任何额外文字、解释、markdown 或换行：
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
                return new ChatResponse { reply = "抱歉，AI 服务暂时不可用，请稍后再试" };
            }

            var aiJson = JObject.Parse(responseText);
            var aiText = aiJson["output"]?["text"]?.ToString() ?? "";

            // 解析 AI 返回的 JSON
            JObject parsed;
            try
            {
                parsed = JObject.Parse(aiText);
            }
            catch
            {
                // 如果不是纯 JSON，尝试提取 JSON 块
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

            // 把 AI 回复加到历史
            messages.Add(new { role = "assistant", content = reply });

            return new ChatResponse
            {
                reply = reply,
                type = type,
                data = data?.ToObject<Dictionary<string, object>>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI 聊天处理异常");
            return new ChatResponse { reply = "服务器内部错误，请稍后再试" };
        }
    }
}
// 输出 DTO
public class ChatResponse
{
    public string reply { get; set; } = string.Empty;
    public string type { get; set; } = "text";
    public Dictionary<string, object>? data { get; set; }
}