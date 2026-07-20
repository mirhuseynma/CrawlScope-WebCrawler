namespace CrawlScope.Application.Modules.Auth.Commands.Login
{
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        public LoginRequestDto Dto { get; set; } = default!;
    }
}
