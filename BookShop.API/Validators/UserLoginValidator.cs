using FluentValidation;
using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for user login requests, ensuring that the email and password fields meet the required criteria.
/// </summary>
public sealed class UserLoginValidator : AbstractValidator<UserLoginDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserLoginDto"/> record and sets up validation rules for the email and password
    /// properties. The email must be in a valid format and cannot be null or empty, while the password must also be provided and
    /// cannot be null or empty. 
    /// </summary>
    public UserLoginValidator()
    {
        RuleFor(x => x.Email)
            .RequiredEmail();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}