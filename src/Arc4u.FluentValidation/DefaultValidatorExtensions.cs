using Arc4u.Data;
using Arc4u.FluentValidation.Rules;
using Arc4u.Results.Validation;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Severity = FluentValidation.Severity;

namespace Arc4u.Validation;

public static class DefaultValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> IsInsert<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsInsertEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsUpdate<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsUpdateEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsDelete<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsDeleteEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsNone<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsNoneEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsUtcDateTime<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : class where TProperty : struct
    {
        return ruleBuilder.SetValidator(new IsUtcDateTimeRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsDateOnly<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : class where TProperty : struct
    {
        return ruleBuilder.SetValidator(new IsUtcDateOnlyRuleValidator<T, TProperty>());
    }

    public static Dictionary<string, object> ToMetadata(this ValidationFailure failure)
    {
        var metadata = new Dictionary<string, object>
        {
            { "Code", failure.ErrorCode },
            { "State", failure.CustomState },
            { "PropertyName", failure.PropertyName },
            { "Severity", failure.Severity }
        };
        return metadata;
    }

    public static List<ValidationError> ToResultErrors(this IEnumerable<ValidationFailure> failures)
    {
        return failures.Select(failure => failure.ToValidationError()).ToList();
    }

    public static ValidationError ToValidationError(this ValidationFailure failure)
    {
        var error = ValidationError.Create(failure.ErrorMessage)
                                   .WithSeverity(failure.Severity.ToSeverity())
                                   .WithCode(failure.ErrorCode);

        return error;
    }

    private static Results.Validation.Severity ToSeverity(this Severity severity)
    {
        return severity switch
        {
            Severity.Error => Results.Validation.Severity.Error,
            Severity.Warning => Results.Validation.Severity.Warning,
            _ => Arc4u.Results.Validation.Severity.Info
        };
    }

    public static async ValueTask<Result<T>> ValidateWithResultAsync<T>(this IValidator<T> validator, T value)
    {
        return await validator.ValidateWithResultAsync(value, CancellationToken.None).ConfigureAwait(false);
    }

    public static async ValueTask<Result<T>> ValidateWithResultAsync<T>(this IValidator<T> validator, T value, CancellationToken cancellation)
    {
        var validationResult = await validator.ValidateAsync(value, cancellation).ConfigureAwait(false);

        if (validationResult.IsValid)
        {
            return Result.Ok(value);
        }

        return Result.Fail(validationResult.Errors.ToResultErrors());
    }

    public static Result<T> ValidateWithResult<T>(this IValidator<T> validator, T value)
    {
        var validationResult = validator.Validate(value);

        if (validationResult.IsValid)
        {
            return Result.Ok(value);
        }

        return Result.Fail(validationResult.Errors.ToResultErrors());
    }

}
