namespace CrawlScope.Application.Modules.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(
        UserManager<AppUser> userManager) : IRequestHandler<ResetPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Dto.Email);
            if (user == null)
            {
                return Result<bool>.Failure("Invalid request.");
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Dto.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.Dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Password reset failed: {errors}");
            }

            return Result<bool>.Success(true);
        }
    }
}
