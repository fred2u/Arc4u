using FluentResults;

namespace Arc4u.Results.Validation;

public class ValidationError : Error
{
    private ValidationError(string message)
    {
        Message = message;
        Code = string.Empty;
        Severity = Severity.Error;
    }

    public static ValidationError Create(string errorMessage)
    {
        return new ValidationError(errorMessage);
    }

    public ValidationError WithSeverity(Severity severity)
    {
        Severity = severity;
        return this;
    }

    public ValidationError WithCode(string code)
    {
        Code = code;
        return this;
    }

    public new ValidationError WithMetadata(string key, object value)
    {
        Metadata.Add(key, value);
        return this;
    }

    public string Code { get; private set; }

    public Severity Severity { get; private set; }

    public static implicit operator Result(ValidationError error)
    {
        return Result.Fail(error);
    }

}

