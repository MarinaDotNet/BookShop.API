using System.ComponentModel;
using FluentValidation;
using MongoDB.Bson;

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

    /// <summary>
    /// Defines validation rules for required text fields.
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
    /// This method is intended for required fields and should be used without a <c>When(...)</c> condition.
    /// </remarks>
    public static IRuleBuilderOptions<T, string> RequiredText<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        int maxLength,
        string fieldName)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage($"{fieldName} cannot be empty.")
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} cannot exceed {maxLength} characters.");
    }

    /// <summary>
    /// Defines validation rules for required collections of strings.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The rule builder used to configure validation rules.
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
    /// <description>The collection is not empty.</description>
    /// </item>
    /// <item>
    /// <description>The collection does not contain null, empty, or whitespace-only values.</description>
    /// </item>
    /// </list>
    /// This method is intended for required collection properties and should be used without a <c>When(...)</c> condition.
    /// </remarks>
    public static IRuleBuilderOptions<T, IReadOnlyCollection<string>> RequiredStringCollection<T>(
        this IRuleBuilderInitial<T, IReadOnlyCollection<string>> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage($"{fieldName} cannot be empty.")
            .Must(values => values.All(value => !string.IsNullOrWhiteSpace(value)))
            .WithMessage($"{fieldName} cannot contain empty or whitespace-only values.");
    }

    /// <summary>
    /// Defines validation rules for required URI fields.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The rule builder used to configure validation rules.
    /// </param>
    /// <returns>
    /// The configured rule builder options.
    /// </returns>
    /// <remarks>
    /// This validation ensures that:
    /// <list type="bullet">
    /// <item>
    /// <description>The URI is not null.</description>
    /// </item>
    /// <item>
    /// <description>The URI is a well-formed absolute URI.</description>
    /// </item>
    /// </list>
    /// This method is intended for required fields and should be used without a <c>When(...)</c> condition.
    /// </remarks> 
    public static IRuleBuilderOptions<T, Uri> RequiredUri<T>(this IRuleBuilderInitial<T, Uri> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Link cannot be null.")
            .Must(link => Uri.IsWellFormedUriString(link?.ToString(), UriKind.Absolute))
            .WithMessage("Link must be a valid URI.");
    }

    /// <summary>
    /// Defines validation rules for optional URI fields.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The rule builder used to configure validation rules.
    /// </param>
    /// <returns>
    /// The configured rule builder options.
    /// </returns>
    /// <remarks>
    /// This validation ensures that:
    /// <list type="bullet">
    /// <item>
    /// <description>If the URI is provided, it must be a well-formed absolute URI.</description>
    /// </item>
    /// <item>
    /// <description>If the URI is null, it is considered valid (since the field is optional).</description>
    /// </item>
    /// </list>
    /// This method is intended for optional fields and should typically be used together with a <c>When(...)</c> condition.
    /// </remarks> 
    public static IRuleBuilderOptions<T, Uri?> OptionalUri<T>(this IRuleBuilderInitial<T, Uri?> ruleBuilder)
    {
        return ruleBuilder
            .Must(link => Uri.IsWellFormedUriString(link?.ToString(), UriKind.Absolute))
            .WithMessage("Link must be a valid URI.");
    }

    /// <summary>
    /// Defines validation rules for required ObjectId fields.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated.
    /// </typeparam>
    /// <param name="ruleBuilder">
    /// The rule builder used to configure validation rules.
    /// </param>
    /// <returns>
    /// The configured rule builder options.
    /// </returns>
    /// <remarks>
    /// This validation ensures that:
    /// <list type="bullet">
    /// <item>
    /// <description>The ObjectId string is not empty.</description>
    /// </item>
    /// <item>
    /// <description>The ObjectId string is a valid ObjectId format.</description>
    /// </item>
    /// </list>
    /// This method is intended for required fields and should be used without a <c>When(...)</c> condition.
    /// </remarks>
    public static IRuleBuilderOptions<T, string> RequiredObjectId<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Id cannot be empty.")
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid ObjectId.");
    }
}