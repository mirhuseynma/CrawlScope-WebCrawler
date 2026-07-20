namespace CrawlScope.Application.Modules.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}
