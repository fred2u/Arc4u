using FluentResults;

namespace Arc4u.Results.Validation;

public static class ValidationExtensions
{

    public static Result<TResult> WithValidationError<TResult>(this Result<TResult> result, string errorMessage)
    {
        return result.WithError(ValidationError.Create(errorMessage));
    }

    public static Result WithValidationError(this Result result, string errorMessage)
    {
        return result.WithError(ValidationError.Create(errorMessage));
    }

}
