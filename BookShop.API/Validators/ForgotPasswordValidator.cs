using FluentValidation;
using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for the <see cref="ForgotPasswordDto"/> record, ensuring that the email provided is valid and properly formatted. 
/// </summary>
public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordValidator"/> class and defines the validation rules for the <see cref="ForgotPasswordDto"/>
    /// object. The validator checks that the email is not null, empty, or whitepace, and that it follows a valid email format.  
    /// </summary>
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .RequiredEmail();
    }
}