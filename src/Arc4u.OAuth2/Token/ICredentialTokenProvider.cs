using Arc4u.OAuth2.Security.Principal;
using FluentResults;

namespace Arc4u.OAuth2.Token;

public interface ICredentialTokenProvider
{
    Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential);
}
