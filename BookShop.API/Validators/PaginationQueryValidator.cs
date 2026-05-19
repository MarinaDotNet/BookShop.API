using BookShop.API.DTOs.Shared;
using FluentValidation;

namespace BookShop.API.Validators;

/// <summary>
/// Validator for pagination query parameters.
/// This class uses FluentValidation to define rules for validating the page number and page size
/// parameters in the PaginationQueryDto. It ensures that the page number is a positive integer and
/// that the page size is within a specified range (between 1 and a defined maximum <see cref="PaginationQueryDto.MaxPageSize"/> ).
/// This validation helps to prevent invalid pagination parameters from being processed by the API,
/// ensuring that clients provide valid input for paginated requests and improving the robustness of the API.
/// </summary>
public sealed class PaginationQueryValidator : AbstractValidator<PaginationQueryDto>
{
    /// <summary>
    /// Defines the validation rules for the PaginationQueryDto properties. This method is called in the constructor of the validator
    /// to set up the validation rules for the page number and page size parameters. It uses FluentValidation's RuleFor method to specify
    /// the validation logic for each property, including custom error messages to provide clear feedback to clients when validation fails.
    /// </summary>
    public PaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be a positive integer.");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationQueryDto.MaxPageSize)
            .WithMessage($"Page size must be between 1 and {PaginationQueryDto.MaxPageSize}.");
    }
}