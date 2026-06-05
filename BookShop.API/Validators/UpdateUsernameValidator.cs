using FluentValidation;
using BookShop.API.DTOs.Auth;
using BookShop.API.Helpers;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for the <see cref="UpdateUsernameDto"/> record, which ensures that the new username provided by the user meets the required
/// criteria for a valid username, such as being a non-empty string and not exceeding the maximum length of 50 characters. 
/// </summary>
public sealed class UpdateUsernameValidator : AbstractValidator<UpdateUsernameDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUsernameValidator"/> class, which defines the validation rules for the
    /// <see cref="UpdateUsernameDto"/> record, specifically validating the <c>NewUserName</c> property to ensure it is a required text field
    /// with a maximum lenth of 50 characters, and providing a user-friendly name for error messages.  
    /// </summary>
    public UpdateUsernameValidator()
    {
        RuleFor(x => x.NewUserName)
            .RequiredText(50, "Username");
    }
}