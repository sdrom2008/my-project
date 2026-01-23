using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MyProject.Infrastructure.AI
{
    public class SemanticKernelConfig
    {
        public Kernel Kernel { get; }

        public SemanticKernelConfig(IConfiguration config)
        {
            var builder = Kernel.CreateBuilder();

            var apiKey = config["Tongyi:Qianwen:ApiKey"]
                         ?? Environment.GetEnvironmentVariable("TONGYI_API_KEY")
                         ?? throw new InvalidOperationException("缺少通义千问 API Key");

            // 通义千问兼容 OpenAI 格式
            builder.AddOpenAIChatCompletion(
                modelId: "qwen-max",
                apiKey: apiKey,
                endpoint: new Uri("https://dashscope.aliyuncs.com/compatible-mode/v1"));

            Kernel = builder.Build();
        }
    }
}