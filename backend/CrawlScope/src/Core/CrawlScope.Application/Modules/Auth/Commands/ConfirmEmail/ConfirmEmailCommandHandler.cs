namespace CrawlScope.Application.Modules.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler(
        UserManager<AppUser> userManager) : IRequestHandler<ConfirmEmailCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                return Result<bool>.Failure("Email confirmation failed.");
            }

            return Result<bool>.Success(true);
        }
    }
}
