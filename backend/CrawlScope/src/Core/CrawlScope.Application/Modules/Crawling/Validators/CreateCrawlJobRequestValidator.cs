using CrawlScope.Application.Modules.Crawling.DTOs;
using FluentValidation;

namespace CrawlScope.Application.Modules.Crawling.Validators
{
    public class CreateCrawlJobRequestValidator : AbstractValidator<CreateCrawlJobRequestDto>
    {
        public CreateCrawlJobRequestValidator()
        {
            RuleFor(x => x.TargetUrl)
                .NotEmpty()
                .WithMessage("Target URL is required.")
                .Must(url =>
                      Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Target URL must be a valid absolute URL.");

            RuleFor(x => x.MaxDepth)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Max Depth must be greater than or equal to 0.");

            RuleFor(x => x.Maxpages)
                .InclusiveBetween(1, 500)
                .WithMessage("Max pages must be between 1 and 500.");
        }
    }
}
