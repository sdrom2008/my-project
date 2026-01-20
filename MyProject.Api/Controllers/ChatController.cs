using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAiChatService _aiService;

    public ChatController(IAiChatService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var reply = await _aiService.ProcessUserMessageAsync(request.Message, _aiService.Get_tools());
        return Ok(new { reply });
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}