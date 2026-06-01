using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;
using FluentValidation;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for user registration requests, ensuring that the username, email, and password fields meet the required criteria.
/// </summary>
public sealed class UserRegisterValidator : AbstractValidator<UserRegisterDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRegisterValidator"/> class and sets up validation rules for the username,
    /// email, and password properties. The username must be provided and cannot be null or empty, the email must be in a valid format
    /// and cannot be null or empty, and the password must meet the defined password requirements and cannot be null or empty. 
    /// </summary>
    public UserRegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");

        RuleFor(x => x.Email)
            .RequiredEmail();

        RuleFor(x => x.Password)
            .RequiredPassword();
    }
}