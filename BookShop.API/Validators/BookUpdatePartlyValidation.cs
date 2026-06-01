using BookShop.API.DTOs.Catalog;
using BookShop.API.Helpers;
using FluentValidation;
using MongoDB.Bson;

namespace BookShop.API.Validators;

/// <summary>
/// Validates partial book update requests.
/// </summary>
/// <remarks>
/// This validator is intended for PATCH-style update operations, where only the provided fields are validated and updated.
/// 
/// Validation rules are only applied to properties that are not null. At least one updatable field must be provided.
/// </remarks>
public sealed class BookUpdatePartlyValidator : AbstractValidator<BookUpdatePartlyDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookUpdatePartlyValidator"/> class and defines validation rules for 
    /// partial book updates.
    /// </summary>
    /// <remarks>
    /// The validator ensures that:
    /// <list type="bullet">
    /// <item>
    /// <description>The book identifier is present and is a valid MondoDB ObjectId.</description>
    /// </item>
    /// <item>
    /// <description>Optional string properties are not empty or whitespace and do not exceed their maximum lengths.</description>
    /// </item>
    /// <item>
    /// <description>Numeric properties contain valid positive values.</description>
    /// </item>
    /// <item>
    /// <description>Collection properties are not empty and do not contain empty or whitespace-only values.</description>
    /// </item>
    /// <item>
    /// <description>Link values are valid absolute URIs.</description>
    /// </item>
    /// <item>
    /// <description>At least one field besides the identifier is provided for update.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public BookUpdatePartlyValidator()
    {
        RuleFor(x => x.Id)
            .RequiredObjectId();

        RuleFor(x => x)
            .Must(HaveAtLeastOneFieldToUpdate)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(x => x.Title)
            .OptionalText(200, "Title")
            .When(x => x.Title is not null);
        
        RuleFor(x => x.Publisher)
            .OptionalText(100, "Publisher")
            .When(x => x.Publisher is not null);

        RuleFor(x => x.Language)
            .OptionalText(50, "Language")
            .When(x => x.Language is not null);

        RuleFor(x => x.Annotation)
            .OptionalText(10000, "Annotation")
            .When(x => x.Annotation is not null);
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Pages)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Pages must be a positive integer.")
            .When(x => x.Pages.HasValue);

        RuleFor(x => x.Authors)
            .OptionalStringCollection("Authors")
            .When(x => x.Authors is not null);

        RuleFor(x => x.Genres)
            .OptionalStringCollection("Genres")
            .When(x => x.Genres is not null);

        RuleFor(x => x.Link)
            .OptionalUri()
            .When(x => x.Link is not null);
    }

    /// <summary>
    /// Determines whether the specified DTO contains at least one field intended for update.
    /// </summary>
    /// <param name="dto">
    /// The partial update DTO to inspect.
    /// </param>
    /// <returns>
    /// <c>true</c> if at least one updatable property contains a value;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The identifier field is ignored during this check. This method is used to prevent empty PATCH requests.
    /// </remarks>
    private static bool HaveAtLeastOneFieldToUpdate(BookUpdatePartlyDto dto)
    {
        return dto.Title is not null 
            || dto.Authors is not null 
            || dto.Genres is not null 
            || dto.Annotation is not null 
            || dto.IsAvailable.HasValue 
            || dto.Language is not null 
            || dto.Link is not null 
            || dto.Pages.HasValue 
            || dto.Price.HasValue 
            || dto.Publisher is not null;
    }
}