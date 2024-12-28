using FluentResults;
using FluentValidation;
using FluentValidation.Results;

namespace Arc4u.Results.Validation;

public class ValidationError : Error
{
    private ValidationError(string message)
    {
        _failure = new ValidationFailure
        {
            Severity = Severity.Error,
            ErrorMessage = message,
            ErrorCode = string.Empty
        };
    }

    public static ValidationError Create(string errorMessage)
    {
        return new ValidationError(errorMessage);
    }

    public ValidationError WithSeverity(Severity severity = Severity.Error)
    {
        _failure.Severity = severity;
        return this;
    }

    public ValidationError WithCode(string code)
    {
        _failure.ErrorCode = code;
        return this;
    }

    public ValidationError(ValidationFailure failure)
    {
        _failure = failure;
        Message = failure.ErrorMessage;
        foreach (var m in failure.ToMetadata())
        {
            Metadata.Add(m.Key, m.Value);
        }
    }

    private readonly ValidationFailure _failure;

    public string Code => _failure.ErrorCode;
    public Severity Severity => _failure.Severity;

    public static implicit operator ValidationError(ValidationFailure failure) => new(failure);

}

