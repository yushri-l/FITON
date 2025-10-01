using Microsoft.AspNetCore.Mvc;
using FITON.Server.Services;
using System.Threading.Tasks;
using FITON.Server.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace FITON.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvatarController : ControllerBase
    {
        private readonly AvatarService _avatarService;

        public AvatarController(AvatarService avatarService)
        {
            _avatarService = avatarService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAvatar([FromBody] AvatarRequestDto avatarRequest)
        {
            if (avatarRequest == null || string.IsNullOrWhiteSpace(avatarRequest.Prompt))
            {
                return BadRequest("Prompt cannot be empty.");
            }

            var result = await _avatarService.GenerateAvatarAsync(avatarRequest.Prompt);

            if (result != null)
            {
                // Parse the JSON result and return it as an object
                try
                {
                    var parsedResult = System.Text.Json.JsonSerializer.Deserialize<object>(result);
                    return Ok(parsedResult);
                }
                catch
                {
                    // If parsing fails, return the raw result
                    return Ok(new { data = result });
                }
            }

            return StatusCode(500, "Failed to generate avatar.");
        }
    }
}
