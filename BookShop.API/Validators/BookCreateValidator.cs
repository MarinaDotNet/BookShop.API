using BookShop.API.DTOs.Catalog;
using BookShop.API.Helpers;
using FluentValidation;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for BookCreateDto.
/// This class uses FluentValidation to define rules for validating the properties of the <see cref="BookCreateDto" />,
/// which is used for creating a new book in the catalog. The validation rules ensure that all required fields are provided
/// and meet specific criteria, such as non-empty strings, valid URIs, and positive numbers.
/// </summary>
public sealed class BookCreateValidator : AbstractValidator<BookCreateDto>
{
    /// <summary>
    /// Defines the validation rules for the <see cref="BookCreateDto"/> properties. Each property is validated according to the
    /// requirements specified in the <see cref="BookCreateDto"/> record.  
    /// </summary>
    public BookCreateValidator()
    {   
        RuleFor(x => x.Title)
            .RequiredText(200, "Title");
        
        RuleFor(x => x.Publisher)
            .RequiredText(100, "Publisher");

        RuleFor(x => x.Language)
            .RequiredText(50, "Language");

        RuleFor(x => x.Annotation)
            .RequiredText(10000, "Annotation");

        RuleFor(x => x.Pages)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Pages must be a positive integer.");
        
        RuleFor(x => x.Price)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be a positive number.");
        
        RuleFor(x => x.Authors)
            .RequiredStringCollection("Authors");

        RuleFor(x => x.Genres)
            .RequiredStringCollection("Genres");

        RuleFor(x => x.Link)
            .RequiredUri();
    }
    
}