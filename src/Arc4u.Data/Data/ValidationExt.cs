using FluentResults;
using Microsoft.Extensions.Logging;

namespace Arc4u.Data;

public static class ValidationExtention
{
    public static Result ValidateAll<T>(this IEnumerable<T> entities, ILogger<T> logger) where T : PersistEntity
    {
        var result = new Result();
        foreach (var entity in entities)
        {
            result.WithReasons(entity.TryValidate().Reasons);
        }

        return result;
    }
}
