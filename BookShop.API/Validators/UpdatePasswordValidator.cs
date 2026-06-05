using FluentValidation;
using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for <see cref="UpdatePasswordDto"/> that ensures the current password is provided and the new password meets the
/// required password criteria defined in the <see cref="ValidationExtensions.RequiredPassword{T}"/> method.  
/// </summary>
public sealed class UpdatePasswordValidator : AbstractValidator<UpdatePasswordDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePasswordValidator"/> class and defines validation rules for updating the account
    /// password of the currently authenticated user. The validation rules ensure that the current password is provided and that the new password
    /// meets the required password criteria, which include being at least 8 characters long, not exceeding 512 characters, and containing at least
    /// one letter and one number. The validation rules are intended for required fields and should be used without a <c>When(...)</c> condition. 
    /// </summary>
    public UpdatePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .RequiredText(512, "Current password");

        RuleFor(x => x.NewPassword)
            .RequiredPassword("New password");
    }
}