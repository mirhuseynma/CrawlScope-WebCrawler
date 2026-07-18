
namespace CrawlScope.Application.Tests.Auth;

public class AuthValidatorTests
{
    [Fact]
    public void RegisterValidate_WhenRequestIsValid_ShouldPass()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequestDto
        {
            FullName = "Test User",
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterValidate_WhenConfirmPasswordDoesNotMatch_ShouldFail()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequestDto
        {
            FullName = "Test User",
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Different123!"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequestDto.ConfirmPassword));
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("test@example.com", "")]
    public void LoginValidate_WhenRequiredFieldsAreMissing_ShouldFail(string emailOrUsername, string password)
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequestDto
        {
            EmailOrUsername = emailOrUsername,
            Password = password
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ForgotPasswordValidate_WhenEmailIsValid_ShouldPass()
    {
        var validator = new ForgotPasswordRequestValidator();
        var request = new ForgotPasswordRequestDto { Email = "test@example.com" };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    public void ForgotPasswordValidate_WhenEmailIsInvalid_ShouldFail(string email)
    {
        var validator = new ForgotPasswordRequestValidator();
        var request = new ForgotPasswordRequestDto { Email = email };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ResetPasswordValidate_WhenRequestIsValid_ShouldPass()
    {
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequestDto
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "Password123!"
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ResetPasswordValidate_WhenPasswordIsWeak_ShouldFail()
    {
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequestDto
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "weak"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ResetPasswordRequestDto.NewPassword));
    }
}
