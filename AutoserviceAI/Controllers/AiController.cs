using AutoserviceAI.BLL.Dto;
using AutoserviceAI.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceAI.API.Controllers
{
    [ApiController]
    [Route("api/ai-diagnostic")]
    public class AiDiagnosticController : ControllerBase
    {
        private readonly IAiDiagnosticService _service;

        public AiDiagnosticController(
            IAiDiagnosticService service)
        {
            _service = service;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequestDto request)
        {
            var reply = await _service.ChatAsync(
                request.Messages);

            return Ok(new
            {
                reply
            });
        }
    }
}
