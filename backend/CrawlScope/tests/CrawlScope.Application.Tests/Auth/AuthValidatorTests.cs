using CrawlScope.Application.Modules.Auth.DTOs;
using CrawlScope.Application.Modules.Auth.Validators;

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
}
