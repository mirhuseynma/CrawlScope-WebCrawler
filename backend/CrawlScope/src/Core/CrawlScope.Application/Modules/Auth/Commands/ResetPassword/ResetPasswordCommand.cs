namespace CrawlScope.Application.Modules.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Result<bool>>
    {
        public ResetPasswordRequestDto Dto { get; set; } = default!;
    }
}
