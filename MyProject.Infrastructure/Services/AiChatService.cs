using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Agent;
using MyProject.Infrastructure.AI;
using MyProject.Infrastructure.Data;
using System.Text.Json;

namespace MyProject.Infrastructure.Services;

public class AiChatService : IAiChatService
{
    private readonly SemanticKernelConfig _skConfig;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly AppDbContext _context;  // 新增注入
    private readonly IIntentClassifier _intentClassifier;
    private readonly AgentRouter _agentRouter;  // 新增

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="skConfig"></param>
    /// <param name="conversationRepository"></param>
    /// <param name="context"></param>
    public AiChatService(
        SemanticKernelConfig skConfig,
        IRepository<Conversation> conversationRepository, AppDbContext context, IIntentClassifier intentClassifier,
        AgentRouter agentRouter)
    {
        _skConfig = skConfig;
        _conversationRepository = conversationRepository;
        _context = context;
        _intentClassifier = intentClassifier;
        _agentRouter = agentRouter;
    }

    /// <summary>
    /// 核心调用方法：处理用户消息并返回 AI 回复
    /// </summary>
    /// <param name="sellerId"></param>
    /// <param name="command"></param>
    /// <returns></returns>
//    public async Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid sellerId, SendChatMessageCommand command)
//    {
//        var agentContext = new AgentContext
//        {
//            SellerId = sellerId,
//            UserMessage = command.Message,
//            CurrentConversation = conversation
//        };
//        var agentResponse = await _productAgent.ExecuteAsync(agentContext);

//        // 1. 获取或创建会话
//        var conversation = await GetOrCreateConversation(sellerId, command.ConversationId);

//        // 2. 添加用户消息
//        var userMsg = ChatMessage.FromUser(command.Message);
//        conversation.AddMessage(userMsg);
//        _context.ChatMessages.Add(userMsg);  // 显式加入跟踪

//        // 3. Semantic Kernel 处理
//        var kernel = _skConfig.Kernel;
//        var chatService = kernel.GetRequiredService<IChatCompletionService>();

//        var chatHistory = new ChatHistory();

//        // 系统 Prompt（固定指令，告诉模型如何处理意图）
//        chatHistory.AddSystemMessage(@"
//你是中国中小电商卖家的智能店小二，精通淘宝/拼多多/抖音运营。

//用户输入一句话意图，你必须严格按照以下规则回复：

//1. 如果用户消息包含“优化”、“详情”、“营销”、“图片”、“标题”、“描述”、“文案”、“脚本”、“卖点”等关键词中的任意一个，判断为“商品优化任务”。

//2. 对于商品优化任务，你必须：
//   - 自动生成优化内容
//   - 只返回严格的 JSON 格式，**禁止添加任何多余文字、解释、markdown、代码块、引号外的任何内容**
//   - JSON 必须完整包含所有字段，即使某些内容为空也要输出空字符串或空数组
//   - 结构必须是：

//{
//  ""replytext"": ""已完成优化，以下是结果"",
//  ""type"": ""optimizeresult"",
//  ""data"": {
//    ""optimizedtitle"": ""这里是优化后的标题（必须是字符串）"",
//    ""optimizeddescription"": ""这里是优化后的描述（支持 Markdown 格式的字符串）"",
//    ""marketingplan"": {
//      ""shortvideoscript"": ""短视频脚本（字符串）"",
//      ""plantingtext"": ""种草文案（字符串）"",
//      ""livescript"": ""直播话术（字符串）"",
//      ""keysellingpoints"": [""卖点1"", ""卖点2"", ""卖点3""]  // 必须是字符串数组，至少1个，最多5个
//    },
//    ""imageprompts"": [""通义万相生成图片的prompt1"", ""prompt2""]  // 必须是字符串数组，至少0个
//  }
//}

//3. 如果不是商品优化任务，返回普通文本回复，格式为：
//{
//  ""replytext"": ""这里是普通回复内容"",
//  ""type"": ""text""
//}

//4. 严格要求：
//   - 所有字段必须存在，不能省略
//   - keysellingpoints 必须是字符串数组，不能是数字、对象、null
//   - 所有值必须是字符串或字符串数组，不能出现数字、布尔值、null（除非数组为空）
//   - 回复必须是纯 JSON，不能有 JSON 外的任何文字、换行、```json 标记

//现在开始处理用户消息。
//");

//        // 加载最近历史（控制 token）
//        foreach (var msg in conversation.Messages.OrderByDescending(m => m.Timestamp).Take(10).Reverse())
//        {
//            chatHistory.AddMessage(msg.IsFromUser ? AuthorRole.User : AuthorRole.Assistant, msg.Content);
//        }

//        // 当前输入
//        chatHistory.AddUserMessage(command.Message);

//        // 执行生成（强制 JSON 输出）
//        var settings = new OpenAIPromptExecutionSettings
//        {
//            Temperature = 0.7,
//            MaxTokens = 3000,
//            ResponseFormat = "json_object"  // 关键：强制 JSON
//        };

//        var result = await chatService.GetChatMessageContentAsync(chatHistory, settings);
//        var replyJson = result.Content ?? "{\"reply_text\":\"抱歉，无法处理\",\"type\":\"text\"}";

//        // 4. 解析 JSON
//        string replyText = "处理中...";
//        string messageType = "text";
//        object? structuredData = null;

//        Console.WriteLine($"JSON 信息: {result.ToString()}\n原始: {replyJson}");

//        try
//        {
//            var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(replyJson);
//            if (parsed != null)
//            {
//                replyText = parsed["replytext"].GetString() ?? replyText;
//                messageType = parsed["type"].GetString() ?? messageType;

//                if (messageType == "optimizeresult" && parsed.TryGetValue("data", out var dataElem))
//                {
//                    try
//                    {
//                        var options = new JsonSerializerOptions
//                        {
//                            PropertyNameCaseInsensitive = true,
//                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // 关键：支持 camelCase
//                            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
//                            AllowTrailingCommas = true,
//                            ReadCommentHandling = JsonCommentHandling.Skip
//                        };

//                        structuredData = JsonSerializer.Deserialize<OptimizeProductData>(dataElem.GetRawText(),
//                                                        new JsonSerializerOptions
//                                                        {
//                                                            PropertyNameCaseInsensitive = true,
//                                                            AllowTrailingCommas = true,
//                                                            ReadCommentHandling = JsonCommentHandling.Skip
//                                                        });

//                        Console.WriteLine("反序列化成功，structuredData: " + JsonSerializer.Serialize(structuredData, options));
//                    }
//                    catch (Exception innerEx)
//                    {
//                        Console.WriteLine("反序列化失败: " + innerEx.Message);
//                        Console.WriteLine("原始 JSON: " + dataElem.GetRawText());
//                    }
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            replyText = $"AI 输出解析失败：{ex.Message}";
//            Console.WriteLine($"JSON 解析错误: {ex.Message}\n原始: {replyJson}");
//        }

//        // 5. 添加 AI 回复
//        var aiMsg = ChatMessage.FromAI(replyText, messageType, structuredData);
//        conversation.AddMessage(aiMsg);
//        _context.ChatMessages.Add(aiMsg);  // 显式加入跟踪

//        // 6. 保存所有变更（关键：如果刚创建，先插入；否则更新）
//        try
//        {
//            Console.WriteLine($"准备保存所有变更，会话ID: {conversation.Id}");

//            // 强制标记为修改（防止跟踪状态混乱）
//            var entry = _context.Entry(conversation);
//            if (entry.State == EntityState.Unchanged)
//            {
//                entry.State = EntityState.Modified;
//            }

//            var rowsAffected = await _conversationRepository.SaveChangesAsync();

//            Console.WriteLine($"保存成功，影响行数: {rowsAffected}（用户消息 + AI 回复）");
//        }
//        catch (DbUpdateConcurrencyException concurrencyEx)
//        {
//            Console.WriteLine($"并发异常: {concurrencyEx.Message}");

//            // 重试：重新加载实体状态，再保存
//            await _context.Entry(conversation).ReloadAsync();
//            var retryRows = await _conversationRepository.SaveChangesAsync();
//            Console.WriteLine($"并发重试保存成功，影响行数: {retryRows}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"最终保存失败: {ex.Message}");
//            if (ex.InnerException != null)
//            {
//                Console.WriteLine($"内层异常: {ex.InnerException.Message}");
//            }
//            throw;  // 继续抛出，便于你看到完整异常
//        }

//        // 7. 返回 DTO
//        return new ChatMessageReplyDto
//        {
//            ConversationId = conversation.Id,
//            Messages = conversation.Messages.Select(m => new ChatMessageItemDto
//            {
//                IsFromUser = m.IsFromUser,
//                Content = m.Content,
//                MessageType = m.Type,
//                Data = m.Type == "optimize_result" && structuredData != null ? structuredData : null
//            }).ToList()
//        };
//    }

    public async Task<ChatMessageReplyDto> ProcessUserMessageAsync(Guid sellerId, SendChatMessageCommand command)
    {
        var conversation = await GetOrCreateConversation(sellerId, command.ConversationId);

        var userMsg = ChatMessage.FromUser(command.Message);
        conversation.AddMessage(userMsg);
        _context.ChatMessages.Add(userMsg);

        // 意图分类
        string intent = await _intentClassifier.ClassifyAsync(command.Message);

        // 构建上下文
        var agentContext = new AgentContext
        {
            SellerId = sellerId,
            UserMessage = command.Message,
            CurrentConversation = conversation
        };

        // 路由到 Agent
        var agentResponse = await _agentRouter.RouteAsync(agentContext, intent);

        // 添加 AI 回复
        var aiMsg = ChatMessage.FromAI(agentResponse.replyText, agentResponse.messageType, agentResponse.data);
        conversation.AddMessage(aiMsg);
        _context.ChatMessages.Add(aiMsg);

        // 保存变更
        await _conversationRepository.SaveChangesAsync();

        // 返回 DTO
        return new ChatMessageReplyDto
        {
            ConversationId = conversation.Id,
            Messages = conversation.Messages.Select(m => new ChatMessageItemDto
            {
                IsFromUser = m.IsFromUser,
                Content = m.Content,
                MessageType = m.Type,
                Data = m.DataJson != null ? JsonSerializer.Deserialize<object>(m.DataJson) : null
            }).ToList()
        };
    }

    private async Task<Conversation> GetOrCreateConversation(Guid sellerId, Guid? conversationId)
    {
        Console.WriteLine($"进入 GetOrCreateConversation，传入 sellerId: {sellerId}");

        Conversation? conversation = null;

        // 尝试查找已有会话
        if (conversationId.HasValue)
        {
            conversation = await _conversationRepository.FirstOrDefaultAsync(
                c => c.Id == conversationId.Value && c.SellerId == sellerId);
            Console.WriteLine($"查找已有会话: {(conversation != null ? "找到" : "未找到")}");
        }

        if (conversation == null)
        {
            Console.WriteLine("创建新会话，SellerId = " + sellerId);
            conversation = Conversation.Create(sellerId);
            Console.WriteLine("新会话内存 Id = " + conversation.Id);

            await _conversationRepository.AddAsync(conversation);

            try
            {
                Console.WriteLine("开始首次保存新会话...");
                await _conversationRepository.SaveChangesAsync();
                Console.WriteLine("新会话首次保存成功，数据库返回 Id: " + conversation.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("首次保存失败: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("内层: " + ex.InnerException.Message);
                }
                throw;
            }
        }

        // 返回会话（此时 Id 已从数据库同步）
        return conversation;
    }
}