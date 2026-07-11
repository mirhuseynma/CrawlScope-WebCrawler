using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlSchedule;

namespace CrawlScope.Application.Modules.Crawling.Validators
{
    public class CreateCrawlScheduleCommandValidator : AbstractValidator<CreateCrawlScheduleCommand>
    {
        public CreateCrawlScheduleCommandValidator()
        {
            RuleFor(x => x.Dto.TargetUrl)
                .NotEmpty()
                .WithMessage("Target URL is required.")
                .Must(url =>
                    Uri.TryCreate(url, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Target URL must be a valid absolute URL.");

            RuleFor(x => x.Dto.MaxDepth)
                .InclusiveBetween(0, 10)
                .WithMessage("Max Depth must be between 0 and 10.");

            RuleFor(x => x.Dto.MaxPages)
                .InclusiveBetween(1, 500)
                .WithMessage("Max pages must be between 1 and 500.");

            RuleFor(x => x.Dto.IntervalMinutes)
                .InclusiveBetween(1, 10080)
                .WithMessage("Interval minutes must be between 1 minute and 7 days.");
        }
    }
}
