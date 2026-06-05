using FluentValidation;
using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for validating the data required to reset a user's password, including the reset token and the new password.
/// </summary>
public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    /// <summary>
    /// The maximum allowed length for the password reset token. This is set to 2048 charactrers to accommodate typical token lengths
    /// while preventing excessively long input that could lead to performance issues or security vulnerabilities.
    /// </summary>
    private const int MaxTokenLength = 2048;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordValidator"/> class and defines the validation rules for the password
    /// reset process. The rules ensure that the reset token is provided and does not exceed the maximum length, and that the new password
    /// meets the required password policy, which includes being of a certain length and containing specific character types. 
    /// </summary>
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .RequiredText(MaxTokenLength, "Token");

        RuleFor(x => x.NewPassword)
            .RequiredPassword("New Password");
    }
}
