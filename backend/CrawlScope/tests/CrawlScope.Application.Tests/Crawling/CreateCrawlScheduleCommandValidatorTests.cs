using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlSchedule;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Application.Modules.Crawling.Validators;

namespace CrawlScope.Application.Tests.Crawling;

public class CreateCrawlScheduleCommandValidatorTests
{
    private readonly CreateCrawlScheduleCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldPass()
    {
        var command = new CreateCrawlScheduleCommand(
            new CreateCrawlScheduleRequestDto
            {
                TargetUrl = "https://example.com",
                MaxDepth = 2,
                MaxPages = 20,
                StayWithinDomain = true,
                IntervalMinutes = 60
            },
            CreatedByUserId: "user-1");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("not-a-url", 1, 10, 60, "Dto.TargetUrl")]
    [InlineData("https://example.com", -1, 10, 60, "Dto.MaxDepth")]
    [InlineData("https://example.com", 11, 10, 60, "Dto.MaxDepth")]
    [InlineData("https://example.com", 1, 0, 60, "Dto.MaxPages")]
    [InlineData("https://example.com", 1, 501, 60, "Dto.MaxPages")]
    [InlineData("https://example.com", 1, 10, 0, "Dto.IntervalMinutes")]
    [InlineData("https://example.com", 1, 10, 10081, "Dto.IntervalMinutes")]
    public void Validate_WhenCommandHasInvalidValues_ShouldFail(
        string targetUrl,
        int maxDepth,
        int maxPages,
        int intervalMinutes,
        string propertyName)
    {
        var command = new CreateCrawlScheduleCommand(
            new CreateCrawlScheduleRequestDto
            {
                TargetUrl = targetUrl,
                MaxDepth = maxDepth,
                MaxPages = maxPages,
                StayWithinDomain = true,
                IntervalMinutes = intervalMinutes
            },
            CreatedByUserId: "user-1");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName);
    }
}
