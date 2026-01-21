using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Interfaces;
using MyProject.Application.DTOs;
using System.Threading.Tasks;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public ChatController(IAiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        var response = await _aiChatService.ChatAsync(request.ConversationId, request.Message);
        return Ok(response);
    }
}