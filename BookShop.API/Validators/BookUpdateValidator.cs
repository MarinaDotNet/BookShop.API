using BookShop.API.DTOs.Catalog;
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
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Title cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Publisher)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Publisher cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Publisher cannot exceed 100 characters.");

        RuleFor(x => x.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Language cannot be empty.")
            .MaximumLength(50)
            .WithMessage("Language cannot exceed 50 characters.");

        RuleFor(x => x.Annotation)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Annotation cannot be empty.")
            .MaximumLength(10000)
            .WithMessage("Annotation cannot exceed 10000 characters.");

        RuleFor(x => x.Price)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.Pages)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Pages must be a positive integer.");

        RuleFor(x => x.Authors)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Authors cannot be empty.")
            .Must(authors => authors.All(author => !string.IsNullOrWhiteSpace(author)))
            .WithMessage("Authors cannot contain empty or whitespace-only names.");

        RuleFor(x => x.Genres)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Genres cannot be empty.")
            .Must(genres => genres.All(genre => !string.IsNullOrWhiteSpace(genre)))
            .WithMessage("Genres cannot contain empty or whitespace-only names.");

        RuleFor(x => x.Link)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Link cannot be null.")
            .Must(link => Uri.IsWellFormedUriString(link.ToString(), UriKind.Absolute))
            .WithMessage("Link must be a valid URI.");

        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Id cannot be empty.")
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid ObjectId.");
    }
}