
using FluentResults;

namespace Arc4u.Security.Principal;

public interface IAppPrincipalFactory
{
    Task<Result<AppPrincipal>> CreatePrincipalAsync(object? parameter = null);
    Task<Result<AppPrincipal>> CreatePrincipalAsync(IKeyValueSettings settings, object? parameter = null);
    Task<Result<AppPrincipal>> CreatePrincipalAsync(string settingsResolveName, object? parameter = null);

    ValueTask SignOutUserAsync(CancellationToken cancellationToken);

    ValueTask SignOutUserAsync(IKeyValueSettings settings, CancellationToken cancellationToken);
}
