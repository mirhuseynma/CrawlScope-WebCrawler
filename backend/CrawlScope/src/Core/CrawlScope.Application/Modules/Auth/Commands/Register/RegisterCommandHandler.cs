namespace CrawlScope.Application.Modules.Auth.Commands.Register
{
    public class RegisterCommandHandler(
        UserManager<AppUser> userManager,
        IEmailService emailService) : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
    {
        public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new AppUser
            {
                UserName = request.Dto.UserName,
                Email = request.Dto.Email,
                FullName = request.Dto.FullName
            };

            var result = await userManager.CreateAsync(user, request.Dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                return Result<AuthResponseDto>.Failure($"User registration failed: {errors}");
            }

            await userManager.AddToRoleAsync(user, "User");
            
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmLink = $"{request.ClientBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

            var body = $@"
                <h3>Confirm Your Account</h3>
                <p>Hello {user.FullName},</p>
                <p>Thank you for registering. Please click the link below to verify your email address:</p>
                <p><a href='{confirmLink}'>Confirm Email</a></p>";

            await emailService.SendEmailAsync(user.Email!, "Confirm Email - CrawlScope", body);

            return Result<AuthResponseDto>.Success(new AuthResponseDto 
            { 
                Token = "",
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FullName = user.FullName
            });
        }
    }
}
