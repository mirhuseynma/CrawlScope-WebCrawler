namespace CrawlScope.Application.Modules.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Dto.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Result<bool>.Success(true);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = $"{request.ClientBaseUrl}/reset-password?email={request.Dto.Email}&token={encodedToken}";

            var body = $@"
                <h3>Reset Your Password</h3>
                <p>Hello {user.FullName},</p>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>";

            await emailService.SendEmailAsync(user.Email!, "Reset Password - CrawlScope", body);

            return Result<bool>.Success(true);
        }
    }
}
