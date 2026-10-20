using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class E2EAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "E2E";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = Request.Headers[E2EIdentities.Header].ToString();

        if (string.IsNullOrWhiteSpace(identity))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!E2EIdentities.Claims.TryGetValue(identity, out var claims))
            return Task.FromResult(AuthenticateResult.Fail($"unknown E2E identity '{identity}'"));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims.Select(claim => new Claim(claim.Type, claim.Value)), SchemeName));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
