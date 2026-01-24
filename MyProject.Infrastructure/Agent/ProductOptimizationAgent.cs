// MyProject.Application/Agents/ProductOptimizationAgent.cs
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyProject.Infrastructure.Agent
{
    public class ProductOptimizationAgent : IAgent
    {
        public string Name => "商品优化";
        public string Description => "优化商品标题、描述、生成营销方案和图片prompt";

        private readonly SemanticKernelConfig _skConfig;

        public ProductOptimizationAgent(SemanticKernelConfig skConfig)
        {
            _skConfig = skConfig;
        }

        public async Task<AgentResponse> ExecuteAsync(AgentContext context, CancellationToken ct = default)
        {
            try
            {
                var kernel = _skConfig.Kernel;
                var chatService = kernel.GetRequiredService<IChatCompletionService>();

                var chatHistory = new ChatHistory();

                // 系统 Prompt（保持你当前的严格 JSON 要求）
                chatHistory.AddSystemMessage(@"
你是中国中小电商卖家的智能店小二，精通淘宝/拼多多/抖音运营。
用户输入一句话意图，你必须返回严格 JSON 格式（无多余文字）。
结构必须是：
{
  ""replytext"": ""已完成优化，以下是结果"",
  ""type"": ""optimize_result"",
  ""data"": {
    ""optimizedtitle"": ""新标题"",
    ""optimizeddescription"": ""Markdown 格式描述"",
    ""marketingplan"": {
      ""shortvideoscript"": ""短视频脚本"",
      ""plantingtext"": ""种草文案"",
      ""livescript"": ""直播话术"",
      ""keysellingpoints"": [""卖点1"", ""卖点2""]
    },
    ""imageprompts"": [""prompt1"", ""prompt2""]
  }
}
所有字段必须存在，数组不能为空字符串或空数组。
");

                // 加载最近 10 条历史
                if (context.CurrentConversation != null)
                {
                    foreach (var msg in context.CurrentConversation.Messages
                        .OrderByDescending(m => m.Timestamp)
                        .Take(10)
                        .Reverse())
                    {
                        chatHistory.AddMessage(msg.IsFromUser ? AuthorRole.User : AuthorRole.Assistant, msg.Content);
                    }
                }

                chatHistory.AddUserMessage(context.UserMessage);

                var settings = new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 3000,
                    ResponseFormat = "json_object"
                };

                //var result = await chatService.GetChatMessageContentAsync(chatHistory, settings, ct);
                var result = await chatService.GetChatMessageContentAsync(
                                chatHistory,
                                settings,
                                kernel: kernel,  // 显式传递 kernel 参数（从 _skConfig.Kernel 获取）
                                cancellationToken: ct
                            );

                var replyJson = result.Content ?? "{\"replytext\":\"抱歉，无法处理\",\"type\":\"text\"}";

                // 解析 JSON（使用之前的容错方式）
                ProductOptimizationResult? data = null;
                string replyText = "处理中...";
                string messageType = "text";

                try
                {
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(replyJson);
                    if (parsed != null)
                    {
                        replyText = parsed["replytext"].GetString() ?? replyText;
                        messageType = parsed["type"].GetString() ?? messageType;

                        if (messageType == "optimize_result" && parsed.TryGetValue("data", out var dataElem))
                        {
                            data = JsonSerializer.Deserialize<ProductOptimizationResult>(
                                dataElem.GetRawText(),
                                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                    }
                }
                catch (Exception ex)
                {
                    replyText = $"AI 输出解析失败：{ex.Message}";
                }

                return new AgentResponse
                {
                    conversationId = context.CurrentConversation?.Id.ToString() ?? "",
                    replyText = replyText,
                    messageType = messageType,
                    success = data != null,
                    data = data
                };
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    replyText = "抱歉，处理出错",
                    messageType = "error",
                    success = false,
                    errorMessage = ex.Message
                };
            }
        }
    }
}