using CrawlScope.Application.Modules.Auth.DTOs;
using FluentValidation;

namespace CrawlScope.Application.Modules.Auth.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.EmailOrUsername)
                .NotEmpty()
                .MaximumLength(160);

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
