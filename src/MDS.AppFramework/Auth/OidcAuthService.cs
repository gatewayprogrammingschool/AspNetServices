using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace MDS.AppFramework.Auth;

public class OidcAuthService
{
    public Task AuthenticateAsync(HttpContext context)
    {
        throw new NotImplementedException("TDD Stub - OIDC Authenticate");
    }

    public Task AuthorizeAsync(AuthorizationHandlerContext context)
    {
        throw new NotImplementedException("TDD Stub - OIDC Authorize");
    }
}