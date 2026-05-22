using FluentValidation;

namespace BookShop.API.Helpers;

/// <summary>
/// Provides reusable FluentValidation extension methods for validating common property patterns.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Defines validation rules for optional text fields.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The rule builder used to configure validation rules.
    /// </param>
    /// <param name="maxLength">
    /// The maximum allowed length of the text.
    /// </param>
    /// <param name="fieldName">
    /// The display name of the validated field used in validation messages.
    /// </param>
    /// <returns>
    /// The configured rule builder options.
    /// </returns>
    /// <remarks>
    /// This validation ensures that:
    /// <list type="bullet">
    /// <item>
    /// <description>The text is not empty or whitespace.</description>
    /// </item>
    /// <item>
    /// <description>The text does not exceed the specified maximum length.</description>
    /// </item>
    /// </list>
    /// 
    /// This method is intended for optional fields and should typically be used together with a <c>When(...)</c> condition.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> OptionalText<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder, 
        int maxLength, 
        string fieldName)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage($"{fieldName} cannot be empty or whitespace.")
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} cannot exceed {maxLength} characters.");
    }

    /// <summary>
    /// Defines validation rules for optional collections of strings.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The rule builder used to configure validation rules.
    /// </param>
    /// <param name="fieldName">
    /// The display name of the validated filed used in validation messages.
    /// </param>
    /// <returns>
    /// The confugured ruled builder options.
    /// </returns>
    /// <remarks>
    /// This validation ensures that:
    /// <list type="bullet">
    /// <item>
    /// <description>The collection is not empty.</description>
    /// </item>
    /// <item>
    /// <description>The collection doesnot contain null, empty, or whitespace-only values.</description>
    /// </item>
    /// </list>
    /// 
    /// This method is intended for optional collection properties and should typically be used together with <c>When(...)</c> condition.
    /// </remarks>
    public static IRuleBuilderOptions<T, IReadOnlyCollection<string>?> OptionalStringCollection<T>(
        this IRuleBuilderInitial<T, IReadOnlyCollection<string>?> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage($"{fieldName} cannot be empty.")
            .Must(values => values!.All(value => !string.IsNullOrWhiteSpace(value)))
            .WithMessage($"{fieldName} cannot contain empty or whitespace-only values.");
    }
}