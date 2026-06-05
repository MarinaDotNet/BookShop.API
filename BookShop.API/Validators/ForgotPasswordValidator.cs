using FluentValidation;
using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;

namespace BookShop.API.Validators;

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .RequiredEmail();
    }
}