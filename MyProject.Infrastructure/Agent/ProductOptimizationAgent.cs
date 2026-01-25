using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using NewtonsoftJson = Newtonsoft.Json.JsonSerializer;   // 别名
using SystemTextJson = System.Text.Json.JsonSerializer;  // 别名
using Newtonsoft.Json.Linq;

namespace MyProject.Infrastructure.Agents
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

                // 统一严格 Prompt
                chatHistory.AddSystemMessage(@"
你是中国中小电商卖家的智能店小二，精通淘宝/拼多多/抖音运营。

用户输入一句话意图，你必须返回严格的 JSON 格式，**禁止添加任何多余文字、markdown、代码块、解释、引号外的任何内容**。

回复必须是纯 JSON，不能有 JSON 外的任何字符。

统一回复结构：

{
  ""replyText"": ""已完成优化，以下是结果"",
  ""messageType"": ""optimize_result"",
  ""data"": {
    ""optimizedTitle"": ""新标题"",
    ""optimizedDescription"": ""Markdown 格式描述"",
    ""marketingPlan"": {
      ""shortVideoScript"": ""短视频脚本"",
      ""plantingText"": ""种草文案"",
      ""liveScript"": ""直播话术"",
      ""keySellingPoints"": [""卖点1"", ""卖点2""]
    },
    ""imagePrompts"": [""prompt1"", ""prompt2""]
  }
}

规则：
- 所有字段必须存在，即使为空也要输出 "" 或 []
- keysellingpoints 必须是字符串数组，至少1个
- 回复必须是纯 JSON
- 如果无法处理，返回 { ""replyText"": ""抱歉，暂时无法处理~"",""messageType"": ""text"",""data"": {} }
");

                // 加载历史
                if (context.CurrentConversation != null)
                {
                    foreach (var msg in context.CurrentConversation.Messages.OrderByDescending(m => m.Timestamp).Take(10).Reverse())
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

                var result = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    settings,
                    kernel: kernel,
                    cancellationToken: ct
                );

                var replyJson = result.Content ?? "{\"replyText\":\"抱歉，无法处理\",\"messageType\":\"text\",\"data\":{}}";


                // 解析 JSON
                ProductOptimizationResult? data = null;
                string replyText = "处理中...";
                string messageType = "text";

                try
                {
                    var parsed = SystemTextJson.Deserialize<Dictionary<string, JsonElement>>(replyJson);
                    if (parsed != null)
                    {
                        replyText = parsed.TryGetValue("replyText", out var rt) ? rt.GetString() ?? replyText : replyText;
                        messageType = parsed.TryGetValue("messageType", out var mt) ? mt.GetString() ?? messageType : messageType;

                        if (messageType == "optimize_result" && parsed.TryGetValue("data", out var dataElem))
                        {
                            try
                            {
                                var dataJson = dataElem.GetRawText();
                                var dataObj = JObject.Parse(dataJson);  // 用 JObject 解析，容错强

                                data = new ProductOptimizationResult
                                {
                                    optimizedTitle = dataObj["optimizedTitle"]?.ToString() ?? "",
                                    optimizedDescription = dataObj["optimizedDescription"]?.ToString() ?? "",

                                    marketingPlan = new MarketingPlan
                                    {
                                        shortVideoScript = dataObj["marketingPlan"]?["shortVideoScript"]?.ToString() ?? "",
                                        plantingText = dataObj["marketingPlan"]?["plantingText"]?.ToString() ?? "",
                                        liveScript = dataObj["marketingPlan"]?["liveScript"]?.ToString() ?? "",
                                        keySellingPoints = dataObj["marketingPlan"]?["keySellingPoints"] is JArray arr
                                            ? arr.Select(t => t.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                                            : new List<string>()
                                    },

                                    imagePrompts = dataObj["imagePrompts"] is JArray imgArr
                                        ? imgArr.Select(t => t.ToString()).ToList()
                                        : new List<string>()
                                };

                                Console.WriteLine("手动解析成功，optimizedTitle: " + data.optimizedTitle);
                                Console.WriteLine("keySellingPoints 数量: " + data.marketingPlan.keySellingPoints.Count);
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine("手动解析失败: " + innerEx.Message);
                                Console.WriteLine("原始 JSON: " + dataElem.GetRawText());

                                // 兜底默认值（确保前端有内容显示）
                                data = new ProductOptimizationResult
                                {
                                    optimizedTitle = "默认优化标题",
                                    optimizedDescription = "默认描述",
                                    marketingPlan = new MarketingPlan
                                    {
                                        keySellingPoints = new List<string> { "默认卖点1", "默认卖点2" }
                                    },
                                    imagePrompts = new List<string> { "默认图片 prompt" }
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    replyText = $"AI 输出解析失败：{ex.Message}";
                    messageType = "error";
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