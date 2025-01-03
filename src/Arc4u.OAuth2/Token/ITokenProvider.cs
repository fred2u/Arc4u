using FluentResults;

namespace Arc4u.OAuth2.Token;

public interface ITokenProvider
{
    Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters);

    ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken);
}
