namespace CrawlScope.Application.Modules.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<AuthResponseDto>>
    {
        public RegisterRequestDto Dto { get; set; } = default!;
        public string ClientBaseUrl { get; set; } = default!;
    }
}
