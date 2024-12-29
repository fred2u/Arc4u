using System.Diagnostics;
using System.Text.Json;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;

namespace Arc4u.OAuth2.TokenProviders;

/// <summary>
/// The purpose of this token provider is to be used by Oidc one.
/// It will refresh a token based on a refresh token and update the scoped TokenRefreshInfo.
/// The Oidc token provider is responsible to get back an access token.
/// </summary>
[Export(typeof(ITokenRefreshProvider))]
public class RefreshTokenProvider(TokenRefreshInfo refreshInfo,
                            IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions,
                            IOptions<OidcAuthenticationOptions> oidcOptions,
                            IActivitySourceFactory activitySourceFactory,
                            ILogger<RefreshTokenProvider> logger) : ITokenRefreshProvider
{
    public const string ProviderName = "Refresh";
    private readonly OidcAuthenticationOptions _oidcOptions = oidcOptions.Value;
    private readonly ActivitySource? _activitySource = activitySourceFactory?.GetArc4u();

    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        ArgumentNullException.ThrowIfNull(refreshInfo);
        ArgumentNullException.ThrowIfNull(openIdConnectOptions);
        ArgumentNullException.ThrowIfNull(_oidcOptions);

        using var activity = _activitySource?.StartActivity("Get on behal of token", ActivityKind.Producer);

        // Check if the token refresh is not expired. 
        // if yes => we have to log this and return a Unauthorized!
        if (DateTime.UtcNow > refreshInfo.RefreshToken.ExpiresOnUtc)
        {
            logger.Technical().LogError($"Refresh token is expired: {refreshInfo.RefreshToken.ExpiresOnUtc}.");
            return new Error("Refreshing the token is impossible, validity date is expired.");
        }

        var options = openIdConnectOptions.Get(OpenIdConnectDefaults.AuthenticationScheme) ?? throw new InvalidCastException("The OpenIdConnectOptions is not found!");
        var metadata = await options!.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);

        var pairs = new Dictionary<string, string>()
                                    {
                                            { "client_id", options.ClientId ?? string.Empty},
                                            { "client_secret", options.ClientSecret ?? string.Empty },
                                            { "grant_type", "refresh_token" },
                                            { "refresh_token", refreshInfo.RefreshToken.Token }
                                    };
        var content = new FormUrlEncodedContent(pairs);

        var tokenResponse = await options.Backchannel.PostAsync(metadata.TokenEndpoint, content, CancellationToken.None).ConfigureAwait(false);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            if (IdentityModelEventSource.ShowPII)
            {
                logger.Technical().LogError($"Refreshing the token is failing. {tokenResponse.ReasonPhrase}");
            }
            else
            {
                logger.Technical().LogError("Refreshing the token is failing. Enable PII to have more info.");
            }
        }
        // throws an exception is not 200OK.
        tokenResponse.EnsureSuccessStatusCode();

        if (tokenResponse.IsSuccessStatusCode)
        {
            using var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
            // Persist the new acess token
            var refresh_token = payload!.RootElement!.GetString("refresh_token");
            var access_token = payload!.RootElement!.GetString("access_token")!;
            if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
            {
                var expirationAt = DateTimeOffset.UtcNow.AddSeconds(seconds).DateTime.ToUniversalTime();
                refreshInfo.AccessToken = new Token.TokenInfo("access_token", access_token, expirationAt);
                if (!string.IsNullOrEmpty(refresh_token))
                {
                    refreshInfo.RefreshToken = new Token.TokenInfo("refresh_token", refresh_token, expirationAt);
                }
            }
            else
            {
                refreshInfo.AccessToken = new Token.TokenInfo("access_token", access_token);
                if (!string.IsNullOrEmpty(refresh_token))
                {
                    refreshInfo.RefreshToken = new Token.TokenInfo("refresh_token", refresh_token, refreshInfo.RefreshToken.ExpiresOnUtc);
                }
            }
        }

        return refreshInfo.AccessToken.ToResult();
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        // there is no Signout on a provider for the token refresh...
        throw new NotImplementedException();
    }
}
