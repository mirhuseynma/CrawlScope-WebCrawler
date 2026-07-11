using CrawlScope.Application.Common.Models;

namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ApiControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await authService.RegisterAsync(request);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await authService.LoginAsync(request);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.ErrorMessage });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = TryGetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var result = await authService.GetCurrentUserAsync(userId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { message = result.ErrorMessage });
        }
    }
}
