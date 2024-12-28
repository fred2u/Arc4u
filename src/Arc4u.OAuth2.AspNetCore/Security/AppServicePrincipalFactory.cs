using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Security.Principal;

[Export(typeof(IAppPrincipalFactory))]
public class AppServicePrincipalFactory(IServiceProvider container, ILogger<AppServicePrincipalFactory> logger, IOptionsMonitor<SimpleKeyValueSettings> settings, IClaimsTransformation claimsTransformation, IActivitySourceFactory activitySourceFactory) : IAppPrincipalFactory
{
    public const string ProviderKey = "ProviderId";

    public static readonly string tokenExpirationClaimType = "exp";
    public static readonly string[] ClaimsToExclude = ["exp", "aud", "iss", "iat", "nbf", "acr", "aio", "appidacr", "ipaddr", "scp", "sub", "tid", "uti", "unique_name", "apptype", "appid", "ver", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant", "http://schemas.microsoft.com/identity/claims/scope"];
    private readonly IOptionsMonitor<SimpleKeyValueSettings> _settings = settings;
    private readonly ActivitySource? _activitySource = activitySourceFactory.GetArc4u();

    public Task<Result<AppPrincipal>> CreatePrincipalAsync(object? parameter = null)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<AppPrincipal>> CreatePrincipalAsync(string settingsResolveName, object? parameter)
    {
        var settings = _settings.Get(settingsResolveName);

        return await CreatePrincipalAsync(settings, parameter).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings">The settings used to create the token.</param>
    /// <param name="messages">Information about the steps to create a token.</param>
    /// <param name="parameter">unused.</param>
    /// <returns></returns>
    /// <exception cref="AppPrincipalException">Thrown when a principal cannot be created.</exception>
    public async Task<Result<AppPrincipal>> CreatePrincipalAsync(IKeyValueSettings settings, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new Result<AppPrincipal>();

        using var activity = _activitySource?.StartActivity("Prepare the creation of the Arc4u Principal", ActivityKind.Producer);

        var identity = new ClaimsIdentity("OAuth2Bearer", "upn", ClaimsIdentity.DefaultRoleClaimType);

        var identityResult = await BuildTheIdentityAsync(identity, settings, parameter).ConfigureAwait(false);
        result.WithReasons(identityResult.Reasons);

        var principal = await claimsTransformation.TransformAsync(new ClaimsPrincipal(identity)).ConfigureAwait(false);

        if (principal is AppPrincipal appPrincipal)
        {
            activity?.SetTag(LoggingConstants.ActivityId, Activity.Current?.Id ?? Guid.NewGuid().ToString());
            result.WithValue(appPrincipal);
            return result;
        }

        return result.WithError("No principal can be created.");
    }

    private async Task<Result> BuildTheIdentityAsync(ClaimsIdentity identity, IKeyValueSettings settings, object? parameter = null)
    {
        // Check if we have a provider registered.
        if (!container.TryGetService(settings.Values[ProviderKey], out ITokenProvider? provider))
        {
            return Result.Fail(new ExceptionalError(new NotSupportedException($"The principal cannot be created. We are missing an account provider: {settings.Values[ProviderKey]}")));
        }

        // Check the settings contains the service url.
        TokenInfo? token = null;
        try
        {
            token = await provider!.GetTokenAsync(settings, parameter).ConfigureAwait(true);
            if (null == token)
            {
                return Result.Fail("The token is null.");
            }
            identity.BootstrapContext = token.Token;
            var jwtToken = new JwtSecurityToken(token.Token);
            identity.AddClaims(jwtToken.Claims.Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.Type))).Select(c => new Claim(c.Type, c.Value)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }

        return Result.Ok();
    }

    private async ValueTask RemoveCacheFromUserAsync()
    {
        if (container.TryGetService<IApplicationContext>(out var appContext))
        {
            if (appContext!.Principal is not null && appContext.Principal.Identity is not null && appContext.Principal.Identity is ClaimsIdentity claimsIdentity)
            {
                var cacheHelper = container.GetService<ICacheHelper>();
                var cacheKeyGenerator = container.GetService<ICacheKeyGenerator>();

                if (null != cacheHelper && null != cacheKeyGenerator)
                {
                    await cacheHelper.GetCache().RemoveAsync(cacheKeyGenerator.GetClaimsKey(claimsIdentity), CancellationToken.None).ConfigureAwait(false);
                }
            }
            else
            {
                logger.Technical().LogError("No principal exists on the current context.");
            }
        }
    }

    public async ValueTask SignOutUserAsync(CancellationToken cancellationToken)
    {
        await RemoveCacheFromUserAsync().ConfigureAwait(false);
    }

    public async ValueTask SignOutUserAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        await RemoveCacheFromUserAsync().ConfigureAwait(false);
    }
}

