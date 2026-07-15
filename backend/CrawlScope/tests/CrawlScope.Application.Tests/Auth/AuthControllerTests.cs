using CrawlScope.Api.Controllers;
using CrawlScope.Application.Abstractions.Auth;
using CrawlScope.Application.Common.Models;
using CrawlScope.Application.Modules.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CrawlScope.Application.Tests.Auth;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);

        // Setup a mock HttpContext to avoid null references when accessing Request.Headers
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Origin"] = "http://localhost:5173";
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task ForgotPassword_WhenServiceReturnsSuccess_ReturnsOk()
    {
        // Arrange
        var request = new ForgotPasswordRequestDto { Email = "test@example.com" };
        _authServiceMock.Setup(s => s.ForgotPasswordAsync(request, It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ForgotPassword_WhenServiceReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordRequestDto { Email = "notfound@example.com" };
        _authServiceMock.Setup(s => s.ForgotPasswordAsync(request, It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Failure("User not found."));

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task ResetPassword_WhenServiceReturnsSuccess_ReturnsOk()
    {
        // Arrange
        var request = new ResetPasswordRequestDto { Email = "test@example.com", Token = "tok", NewPassword = "Password123!" };
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ConfirmEmail_WhenValid_ReturnsOk()
    {
        // Arrange
        var userId = "user-123";
        var token = "valid-token";
        _authServiceMock.Setup(s => s.ConfirmEmailAsync(userId, token))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ConfirmEmail(userId, token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ConfirmEmail_WhenMissingParameters_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ConfirmEmail("", "token");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}
