
namespace CrawlScope.Application.Modules.Crawling.Validators
{
    public class CreateCrawlJobCommandValidator : AbstractValidator<CreateCrawlJobCommand>
    {
        public CreateCrawlJobCommandValidator(IValidator<DTOs.CreateCrawlJobRequestDto> requestValidator)
        {
            RuleFor(x => x.Dto).SetValidator(requestValidator);
        }
    }
}
