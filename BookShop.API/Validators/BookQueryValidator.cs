using BookShop.API.DTOs.Catalog;
using FluentValidation;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for BookQueryDto.
/// This class uses FluentValidation to define rules for validating the properties of the BookQueryDto, which is used for querying books
/// with various filtering and sorting criteria. The validation rules ensure that the SortBy property, if provided, is one of the allowed
/// fields (Title, Price, Publisher) and that the MinPrice and MaxPrice properties, if provided, are
/// greater than or equal to 0. Additionally, it validates that the BookQueryDto itself is not null or empty. This validation helps to
/// ensure that clients provide valid input when querying the book catalog, improving the robustness and reliability of the API.
/// </summary>
public sealed class BookQueryValidator : AbstractValidator<BookQueryDto>
{
    /// <summary>
    /// Defines the allowed fields for sorting books in the catalog. This is a static readonly HashSet of strings that contains the names
    /// of the properties of the book entity that can be used for sorting when querying the book catalog. The HashSet is initialized with
    /// a list of allowed sort fields (Title, Price, Publisher) and uses a case-insensitive string comaprer
    /// to ensure that the validation of the SortBy property in the BookQueryDto is not affected by the case of the input. 
    /// </summary>
    public static readonly HashSet<string> AllowedSortFields = 
        new ([
            "Title", "Price", "Publisher"
        ], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Defines the validation rules for the BookQueryDto properties.
    /// </summary>
    public BookQueryValidator()
    {
        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortFields.Contains(sortBy))
            .WithMessage("SortBy must be one of the following: Title, Price, Publisher.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinPrice must be greater than or equal to 0.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxPrice must be greater than or equal to 0.");

        RuleFor(x => x)
            .NotNull()
            .NotEmpty()
            .WithMessage("BookQueryDto cannot be null.");
    }
}