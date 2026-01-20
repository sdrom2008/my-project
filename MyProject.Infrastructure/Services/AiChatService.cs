using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyProject.Application.Plugins;
//using MyProject.Application.Services;
using MyProject.Infrastructure.AIServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyProject.Infrastructure.Services;

public class AiChatService : MyProject.Application.Services.IAiChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public AiChatService(IKernelService kernelService)
    {
        _kernel = kernelService.GetKernel();

        // 导入插件（工具）
        _kernel.ImportPluginFromType<CustomerServicePlugin>();

        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> ProcessUserMessageAsync(string userMessage, string? conversationId = null)
    {
        // 创建聊天历史（后期用 Memory 或 Redis 持久化多轮）
        var history = new ChatHistory();
        history.AddSystemMessage("你是一个专业、友好的中文智能客服助手。帮助用户查询订单、退款、物流等。用简洁中文回复。如果需要查询订单，使用提供的工具。");

        history.AddUserMessage(userMessage);

        // 执行设置：启用自动 function calling
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // 获取回复（SK 会自动调用工具如果需要）
        var result = await _chatService.GetChatMessageContentAsync(
            history,
            executionSettings: executionSettings,
            kernel: _kernel);

        return result.Content ?? "抱歉，暂时无法处理，请稍后再试。";
    }
}