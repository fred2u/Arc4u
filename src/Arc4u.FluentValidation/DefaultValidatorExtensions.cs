using Arc4u.Data;
using Arc4u.FluentValidation.Rules;
using FluentValidation;

namespace Arc4u.FluentValidation;

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
}
