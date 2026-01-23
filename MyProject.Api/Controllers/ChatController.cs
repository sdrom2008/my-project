using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Interfaces;
using MyProject.Domain.Entities;

[ApiController]
[Route("api/chat")]
[Authorize]  // 所有接口都需要登录（Seller 身份）
public class ChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public ChatController(IAiChatService aiChatService)
    {
        _aiChatService = aiChatService ?? throw new ArgumentNullException(nameof(aiChatService));
    }

    /// <summary>
    /// 发送聊天消息（支持商品优化、营销方案、智能客服等意图）
    /// </summary>
    /// <param name="command">聊天输入（消息内容 + 可选会话ID）</param>
    /// <returns>聊天回复（包含结构化结果，如商品优化数据）</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ChatMessageReplyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] SendChatMessageCommand command)
    {
        // 1. 参数校验
        if (command == null || string.IsNullOrWhiteSpace(command.Message))
        {
            return BadRequest("消息内容不能为空");
        }

        // 2. 从 JWT 获取当前商家 ID
        var sellerIdClaim = User.FindFirst("sellerId")?.Value;
        if (string.IsNullOrEmpty(sellerIdClaim) || !Guid.TryParse(sellerIdClaim, out var sellerId))
        {
            return Unauthorized("无效的商家身份");
        }

        // 3. 调用 Application 层服务处理（核心业务逻辑在这里）
        var reply = await _aiChatService.ProcessUserMessageAsync(sellerId, command);

        // 4. 返回结构化结果
        return Ok(reply);
    }

    /// <summary>
    /// 获取指定会话的完整历史消息（用于前端加载历史聊天记录）
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <returns>会话消息列表</returns>
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetConversationHistory(Guid conversationId)
    {
        var sellerIdClaim = User.FindFirst("sellerId")?.Value;
        if (!Guid.TryParse(sellerIdClaim, out var sellerId))
        {
            return Unauthorized();
        }

        // 这里可以调用仓储或服务获取历史（暂时简单实现，后续完善）
        // 示例：假设你有 IConversationRepository
        // var conversation = await _conversationRepository.GetWithMessagesAsync(conversationId);
        // if (conversation == null || conversation.SellerId != sellerId) return NotFound();

        // return Ok(... 转 DTO);

        return Ok(new { message = "历史消息加载功能开发中" });
    }
}