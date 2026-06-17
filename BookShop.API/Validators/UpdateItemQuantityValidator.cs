using BookShop.API.DTOs.Catalog;
using FluentValidation;

namespace BookShop.API.Validators;

/// <summary>
/// Validates <see cref="UpdateItemQuantityDto"/> before updating an item quantity in the cart.
/// Ensures the quantity is at least 1. 
/// </summary>
public sealed class UpdateItemQuantityValidator : AbstractValidator<UpdateItemQuantityDto>
{
    public UpdateItemQuantityValidator()
    {
        RuleFor(x => x.Quantity)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Quantity must be at least 1.");
    }
}