using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using MyProject.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Infrastructure.Services;  // 放 Infrastructure，因为依赖外部 API

public class AiChatService : MyProject.Application.Services.IAiChatService
{
    private readonly ChatClient _chatClient;
    private readonly List<ChatTool> _tools;

    public AiChatService(IConfiguration config)
    {
        var apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI ApiKey missing");
        var model = config["OpenAI:Model"] ?? "gpt-4o-mini";

        var openAiClient = new OpenAIClient(apiKey);
        _chatClient = openAiClient.GetChatClient(model);

        // 定义工具（function calling）：模拟查订单
        _tools = new List<ChatTool>
        {
            ChatTool.CreateFunctionTool(
                functionName: "get_order_status",
                functionDescription: "查询用户订单状态，需要订单ID或手机号",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "orderId": { "type": "string", "description": "订单号" },
                        "phone": { "type": "string", "description": "用户手机号" }
                    },
                    "required": ["orderId"]
                }
                """)
            )
        };
    }

    public List<ChatTool> Get_tools()
    {
        return _tools;
    }

    public async Task<string> ProcessUserMessageAsync(string userMessage, List<ChatTool> _tools, string? conversationId = null)
    {
        // 简单起见，先不存 conversation history（后期用 Redis 或 DB 存）
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("你是一个智能客服助手，帮助用户查询订单、退款等。用中文回复，友好专业。"),
            new UserChatMessage(userMessage)
        };

        var options = new ChatCompletionOptions
        {
            Tools = {
                _tools
            },
            // ToolChoice = "auto"  // 默认 auto，让模型决定是否调用工具
        };

        var response = await _chatClient.CompleteChatAsync(messages, options);

        // 处理工具调用（如果模型决定调用 function）
        if (response.Value.ToolCalls.Count > 0)
        {
            foreach (var toolCall in response.Value.ToolCalls)
            {
                if (toolCall.FunctionName == "get_order_status")
                {
                    // 解析参数（实际生产用 JsonSerializer）
                    var args = toolCall.FunctionArguments.ToObjectFromJson<Dictionary<string, string>>();
                    var orderId = args?["orderId"];

                    // 模拟数据库查询
                    var fakeStatus = orderId switch
                    {
                        "ORD123" => "已发货，预计明天到达",
                        _ => "未找到订单，请提供正确订单号"
                    };

                    // 把工具结果加回消息，继续生成最终回复
                    messages.Add(new AssistantChatMessage(toolCall));
                    messages.Add(new ToolChatMessage(toolCall.Id, $"订单状态：{fakeStatus}"));

                    // 再调用一次 LLM 生成自然回复
                    response = await _chatClient.CompleteChatAsync(messages, options);
                }
            }
        }

        return response.Value.Content[0].Text ?? "抱歉，我暂时无法处理，请稍后再试。";
    }

    public Task<string> ProcessUserMessageAsync(string userMessage, string? conversationId = null)
    {
        throw new NotImplementedException();
    }
}