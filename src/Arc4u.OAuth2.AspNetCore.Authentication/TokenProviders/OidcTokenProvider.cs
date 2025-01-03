using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using FluentResults;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProviders;

[Export(OidcTokenProvider.ProviderName, typeof(ITokenProvider))]
public class OidcTokenProvider(TokenRefreshInfo tokenRefreshInfo, IOptions<OidcAuthenticationOptions> oidcOptions, ITokenRefreshProvider refreshTokenProvider) : ITokenProvider
{
    public const string ProviderName = "Oidc";
    private readonly OidcAuthenticationOptions _oidcOptions = oidcOptions.Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="platformParameters"></param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException" />
    /// <returns><see cref="TokenInfo"/></returns>
    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var timeRemaining = tokenRefreshInfo.AccessToken.ExpiresOnUtc.Subtract(DateTime.UtcNow);

        if (timeRemaining > _oidcOptions.ForceRefreshTimeoutTimeSpan)
        {
            return tokenRefreshInfo.AccessToken.ToResult();
        }

        return await refreshTokenProvider.GetTokenAsync(settings, null).ConfigureAwait(false);
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
