﻿using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Security;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Token.Adal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace Arc4u.OAuth2.TokenProvider;

[Export(AdalTokenProvider.ProviderName, typeof(ITokenProvider))]
public class AdfsTokenProvider : AdalTokenProvider
{

    public AdfsTokenProvider(IUserObjectIdentifier userCacheKeyIdentifier, ITokenUserCacheConfiguration tokenUserCacheConfiguration, ILogger<AdfsTokenProvider> logger, IContainerResolve container) : base(userCacheKeyIdentifier, tokenUserCacheConfiguration, logger, container) { }


    protected override AuthenticationContext CreateAuthenticationContext(string authority, string cacheIdentifier)
    {
        return new AuthenticationContext(authority, false, new Cache(_logger, Container, cacheIdentifier));
    }
}