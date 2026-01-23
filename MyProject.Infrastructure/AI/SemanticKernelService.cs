using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace MyProject.Infrastructure.AI;

public class SemanticKernelService
{
    private readonly Kernel _kernel;

    public SemanticKernelService(IConfiguration config)
    {
        var builder = Kernel.CreateBuilder();

        // 通义千问兼容 OpenAI 格式
        var endpoint = new Uri("https://dashscope.aliyuncs.com/compatible-mode/v1");
        var apiKey = config["Tongyi:Qianwen:ApiKey"] ?? throw new InvalidOperationException("缺少通义 Key");

        builder.AddOpenAIChatCompletion(
            modelId: "qwen-max",           // 或 qwen-turbo、qwen-plus
            apiKey: apiKey,
            endpoint: endpoint);

        // 未来可加其他模型
        // builder.AddOpenAIChatCompletion("gpt-4o", "...");

        _kernel = builder.Build();
    }

    public Kernel GetKernel() => _kernel;

    public IChatCompletionService GetChatService() => _kernel.GetRequiredService<IChatCompletionService>();
}