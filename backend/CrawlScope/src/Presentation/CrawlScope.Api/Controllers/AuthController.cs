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
            var origin = Request.Headers["Origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = "http://localhost:5173";
            }
            var result = await authService.RegisterAsync(request, origin);
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var origin = Request.Headers["Origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = "http://localhost:5173"; // fallback
            }

            var result = await authService.ForgotPasswordAsync(request, origin);
            return result.IsSuccess ? Ok(new { message = "Password reset link sent." }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await authService.ResetPasswordAsync(request);
            return result.IsSuccess ? Ok(new { message = "Password reset successful." }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Invalid email confirmation request." });
            }

            var result = await authService.ConfirmEmailAsync(userId, token);
            return result.IsSuccess ? Ok(new { message = "Email confirmed successfully." }) : BadRequest(new { message = result.ErrorMessage });
        }
    }
}
