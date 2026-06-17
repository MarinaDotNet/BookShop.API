using BookShop.API.DTOs.Catalog;
using BookShop.API.Helpers;
using FluentValidation;

namespace BookShop.API.Validators;

/// <summary>
/// Validates <see cref="AddToCartDto"/> before adding a book to the cart.
/// Ensures the book identifier is a valid ObjectId and the quantity is at least 1. 
/// </summary>
public sealed class AddToCartValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.BookId)
            .RequiredObjectId();
        
        RuleFor(x => x.Quantity)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Quantity must be at least 1.");
    }
}