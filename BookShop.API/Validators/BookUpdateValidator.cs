using BookShop.API.DTOs.Catalog;
using BookShop.API.Helpers;
using FluentValidation;
using MongoDB.Bson;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for BookUpdateDto.
/// This class uses FluentValidation to define rules for validating the properties of the <see cref="BookUpdateDto"/> ,
/// which is used for updating an existing book in the catalog. The validation rules ensure that all required fields are provided
/// and meet specific criteria, such as non-empty strings, valid URIs, and valid ObjectId formats. This validation helps to ensure
/// that clients provide valid input when updating a book, improving the robustness and reliability of the API.
/// </summary>
public sealed class BookUpdateValidator : AbstractValidator<BookUpdateDto>
{
    /// <summary>
    /// Defines the validation rules for the <see cref="BookUpdateDto"/> properties. Each property is validated according to the requirements
    /// specified in the <see cref="BookUpdateDto"/> record, such as non-empty strings for Title, Publisher, and Language, valid URIs for Link,
    /// and valid ObjectId formats for Id. The validation rules are defined using FluentValidation's RuleFor method, and appropriate
    /// error messages are provided for each validation failure case. This constructor sets up the validation logic that will be applied
    /// when validating instances of <see cref="BookUpdateDto"/>.
    /// </summary>
    public BookUpdateValidator()
    {   
        RuleFor(x => x.Title)
            .RequiredText(200, "Title");

        RuleFor(x => x.Publisher)
            .RequiredText(100, "Publisher");

        RuleFor(x => x.Language)
            .RequiredText(50, "Language");

        RuleFor(x => x.Annotation)
            .RequiredText(10000, "Annotation");

        RuleFor(x => x.Price)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.Pages)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Pages must be a positive integer.");

        RuleFor(x => x.Authors)
            .RequiredStringCollection("Authors");

        RuleFor(x => x.Genres)
            .RequiredStringCollection("Genres");

        RuleFor(x => x.Link)
            .RequiredUri();

        RuleFor(x => x.Id)
            .RequiredObjectId();
    }
}