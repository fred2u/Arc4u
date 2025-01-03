using Arc4u.Diagnostics;
using Arc4u.Results.Validation;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Arc4u.Results.Logging;

public class FluentLogger : IResultLogger
{
    public FluentLogger(ILogger<FluentLogger> logger)
    {
        _logger = logger;
    }
    delegate CommonLoggerProperties logDelegate(string message, params object[] args);

    private readonly ILogger<FluentLogger> _logger;
    public void Log(string context, string content, ResultBase result, LogLevel logLevel)
    {
        logDelegate? logger;
        if (!string.IsNullOrEmpty(context) && !string.IsNullOrEmpty(content))
        {
            logger = GetBusinessLogger(logLevel);

            logger(content).AddIf(context is not null, "Context", () => context!).Log();
        }

        if (result is not null && result.IsFailed && result.Errors is not null)
        {
            foreach (var error in result.Errors)
            {
                if (error is ValidationError validationError)
                {
                    logger = GetBusinessLogger(validationError.Severity);

                    logger(validationError.Message)
                        .AddIf(validationError.Code is not null, "Code", () => validationError.Code!)
                        .Log();

                    LogReasons(result.Reasons);
                    continue;
                }
                if (error is IExceptionalError exceptionalError)
                {
                    _logger.Technical().Exception(exceptionalError.Exception).Log();
                    continue;
                }

                logger = GetBusinessLogger(LogLevel.Error);
                logger(error.Message).Log();
            }
        }

        if (result is not null && result.IsSuccess && result.Reasons.Any())
        {
            LogReasons(result.Reasons);
        }
    }

    public void Log<TContext>(string content, ResultBase result, LogLevel logLevel)
    {
        var logger = GetBusinessLogger(logLevel);

        if (!string.IsNullOrEmpty(content))
        {
            logger(content).Add("Context", typeof(TContext).FullName!).Log();
        }

        if (result is not null && result.IsFailed && result.Errors is not null)
        {
            foreach (var error in result.Errors)
            {
                if (error is ValidationError validationError)
                {
                    logger = GetBusinessLogger(validationError.Severity);

                    logger(validationError.Message)
                        .AddIf(validationError.Code is not null, "Code", () => validationError.Code!)
                        .Log();

                    LogReasons(result.Reasons);
                    continue;
                }
                if (error is IExceptionalError exceptionalError)
                {
                    _logger.Technical().Exception(exceptionalError.Exception).Log();
                    continue;
                }

                logger = GetBusinessLogger(LogLevel.Error);
                logger(error.Message).Log();
            }
        }

        if (result is not null && result.IsSuccess && result.Reasons.Any())
        {
            LogReasons(result.Reasons);
        }
    }

    private void LogReasons(IList<IReason> reasons)
    {
        foreach (var reason in reasons)
        {
            var logger = GetBusinessLogger(LogLevel.Information);

            logger(reason.Message)
                .Log();
        }
    }

    private logDelegate GetBusinessLogger(Severity severity) => severity switch
    {
        Severity.Error => _logger.Business().Error,
        Severity.Warning => _logger.Business().Warning,
        Severity.Info => _logger.Business().Information,
        _ => _logger.Business().Debug,
    };

    private logDelegate GetBusinessLogger(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => _logger.Business().Debug,
        LogLevel.Debug => _logger.Business().Debug,
        LogLevel.Information => _logger.Business().Information,
        LogLevel.Warning => _logger.Business().Warning,
        LogLevel.Error => _logger.Business().Error,
        LogLevel.Critical => _logger.Business().Fatal,
        LogLevel.None => _logger.Business().Debug,
        _ => _logger.Business().Debug,
    };
}
