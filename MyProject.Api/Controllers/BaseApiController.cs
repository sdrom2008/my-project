using Microsoft.AspNetCore.Mvc;

namespace MyProject.Api.Controllers
{
    public class BaseApiController : ControllerBase
    {
        protected IActionResult HandleError(Exception ex)
        {
            return StatusCode(500, new { message = "服务器内部错误", error = ex.Message });
        }
    }
}
