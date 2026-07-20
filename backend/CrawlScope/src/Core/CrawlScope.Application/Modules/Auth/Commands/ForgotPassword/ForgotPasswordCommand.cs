namespace CrawlScope.Application.Modules.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<Result<bool>>
    {
        public ForgotPasswordRequestDto Dto { get; set; } = default!;
        public string ClientBaseUrl { get; set; } = default!;
    }
}
