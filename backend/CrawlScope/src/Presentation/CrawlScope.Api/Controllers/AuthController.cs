
namespace CrawlScope.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ApiControllerBase
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
            
            var command = new RegisterCommand { Dto = request, ClientBaseUrl = origin };
            var result = await mediator.Send(command, cancellationToken);
            
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand { Dto = request };
            var result = await mediator.Send(command, cancellationToken);
            
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

            var query = new GetCurrentUserQuery { UserId = userId };
            var result = await mediator.Send(query);
            
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

            var command = new ForgotPasswordCommand { Dto = request, ClientBaseUrl = origin };
            var result = await mediator.Send(command);
            
            return result.IsSuccess ? Ok(new { message = "Password reset link sent." }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var command = new ResetPasswordCommand { Dto = request };
            var result = await mediator.Send(command);
            
            return result.IsSuccess ? Ok(new { message = "Password reset successful." }) : BadRequest(new { message = result.ErrorMessage });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Invalid email confirmation request." });
            }

            var command = new ConfirmEmailCommand { UserId = userId, Token = token };
            var result = await mediator.Send(command);
            
            return result.IsSuccess ? Ok(new { message = "Email confirmed successfully." }) : BadRequest(new { message = result.ErrorMessage });
        }
    }
}
