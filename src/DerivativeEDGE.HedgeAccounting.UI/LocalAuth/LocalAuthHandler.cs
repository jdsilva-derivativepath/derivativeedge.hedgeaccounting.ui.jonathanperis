namespace DerivativeEDGE.HedgeAccounting.UI.LocalAuth;

/// <summary>
/// Authentication Handler for local development that does not require reverse proxy to be running
/// </summary>
public class LocalAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //TODO: configure local claims as needed
        var claims = new[] { new Claim("ClientId", "1"),
                             new Claim("givenname", "Bob"),
                             new Claim("surname", "Barker"),
                             new Claim("name", "bbarker@priceisright.test"),
                             new Claim("edgeuserid", "0")
                            };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
