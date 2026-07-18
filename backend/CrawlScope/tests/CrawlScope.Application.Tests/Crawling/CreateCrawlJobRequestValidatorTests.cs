
namespace CrawlScope.Application.Tests.Crawling;

public class CreateCrawlJobRequestValidatorTests
{
    private readonly CreateCrawlJobRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldPass()
    {
        var request = new CreateCrawlJobRequestDto
        {
            TargetUrl = "https://example.com",
            MaxDepth = 2,
            MaxPages = 25,
            StayWithinDomain = true
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    public void Validate_WhenTargetUrlIsInvalid_ShouldFail(string targetUrl)
    {
        var request = new CreateCrawlJobRequestDto
        {
            TargetUrl = targetUrl,
            MaxDepth = 1,
            MaxPages = 10,
            StayWithinDomain = true
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCrawlJobRequestDto.TargetUrl));
    }

    [Theory]
    [InlineData(-1, 10, nameof(CreateCrawlJobRequestDto.MaxDepth))]
    [InlineData(11, 10, nameof(CreateCrawlJobRequestDto.MaxDepth))]
    [InlineData(1, 0, nameof(CreateCrawlJobRequestDto.MaxPages))]
    [InlineData(1, 501, nameof(CreateCrawlJobRequestDto.MaxPages))]
    public void Validate_WhenLimitsAreOutsideAllowedRange_ShouldFail(int maxDepth, int maxPages, string propertyName)
    {
        var request = new CreateCrawlJobRequestDto
        {
            TargetUrl = "https://example.com",
            MaxDepth = maxDepth,
            MaxPages = maxPages,
            StayWithinDomain = true
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName);
    }
}
