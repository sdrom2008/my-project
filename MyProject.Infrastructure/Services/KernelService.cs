using Cnblogs.SemanticKernel.Connectors.DashScope;  // 注意命名空间
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MyProject.Application.Interfaces;
using MyProject.Infrastructure.AIServices;

namespace MyProject.Infrastructure.Services;

public class KernelService : IKernelService
{
    private readonly Kernel _kernel;

    public KernelService(IConfiguration config)
    {
        var builder = Kernel.CreateBuilder();

        // 配置 DashScope Connector
        var apiKey = config["DashScope:ApiKey"] ?? throw new InvalidOperationException("DashScope ApiKey missing");
        var modelId = config["DashScope:ModelId"] ?? "qwen-max";  // 默认用旗舰模型

        // 添加 DashScope Chat Completion
        //builder.AddDashScopeChatCompletion(modelId, apiKey);
        builder.Services.AddDashScopeChatCompletion(config, modelId, apiKey);

        // 如果需要 Embedding（后期 RAG 用）
        // builder.AddDashScopeTextEmbedding("text-embedding-v2", apiKey);

        // 可选：日志
        builder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug));

        _kernel = builder.Build();
    }

    public Kernel GetKernel() => _kernel;
}