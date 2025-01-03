using FluentResults;

namespace Arc4u.Results.Validation;

public static class ValidationExtensions
{

    public static Result<TResult> WithValidationError<TResult>(this Result<TResult> result, string errorMessage)
    {
        return result.WithError(ValidationError.Create(errorMessage));
    }

    public static Result<TResult> WithValidationError<TResult>(this Result<TResult> result, string errorMessage, string code)
    {
        return result.WithError(ValidationError.Create(errorMessage).WithCode(code));
    }

    public static Result<TResult> WithValidationError<TResult>(this Result<TResult> result, string errorMessage, Severity severity)
    {
        return result.WithError(ValidationError.Create(errorMessage).WithSeverity(severity));
    }

    public static Result<TResult> WithValidationError<TResult>(this Result<TResult> result, string errorMessage, string code, Severity severity)
    {
        return result.WithError(ValidationError.Create(errorMessage).WithCode(code).WithSeverity(severity));
    }

    public static Result WithValidationError(this Result result, string errorMessage)
    {
        return result.WithError(ValidationError.Create(errorMessage));
    }

    public static Result WithValidationError(this Result result, string errorMessage, string code)
    {
        return result.WithError(ValidationError.Create(errorMessage).WithCode(code));
    }

    public static Result WithValidationError(this Result result, string errorMessage, Severity severity)
    {
        return result.WithError(ValidationError.Create(errorMessage).WithSeverity(severity));
    }

    public static Result WithValidationError(this Result result, string errorMessage, string code, Severity severity)
    {
        return result.WithError(ValidationError.Create(errorMessage).WithCode(code).WithSeverity(severity));
    }
}
