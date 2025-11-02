namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Middleware;

public class JwtTokenForwardHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : HttpClientHandler
{
    private const string AccessTokenHeader = "access_token"; // TODO: Consider renaming to 'X-HA-AccessToken'.

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        string existingToken = context.Request.Headers[AccessTokenHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(existingToken) || IsExpired(existingToken))
        {
            try
            {
                existingToken = GenerateJwtFromCurrentUserClaims();
                if (context.Request.Headers.ContainsKey(AccessTokenHeader))
                {
                    context.Request.Headers.Remove(AccessTokenHeader);
                }
                context.Request.Headers.Add(AccessTokenHeader, existingToken);
            }
            catch (Exception)
            {
                // Swallow generation errors (optionally log later). Leave existingToken null so no Authorization header is added.
                existingToken = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(existingToken))
        {
            if (!request.Headers.Accept.Any(h => h.MediaType == "application/json"))
            {
                request.Headers.Accept.ParseAdd("application/json");
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private bool IsExpired(string token)
    {
        try
        {
            var jwt = new JwtSecurityToken(token);
            return jwt.ValidTo <= DateTime.UtcNow;
        }
        catch
        {
            return true; // Treat invalid token as expired.
        }
    }

    private string GenerateJwtFromCurrentUserClaims()
    {
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext available.");
        var claims = context.User?.Claims?.ToArray() ?? Array.Empty<Claim>();
        var keyString = configuration["JWT_ISSUERSIGNINGKEY"] ?? throw new InvalidOperationException("JWT_ISSUERSIGNINGKEY not configured.");
        var issuer = configuration["JWT_ISSUER"];
        var audience = configuration["JWT_AUDIENCE"];

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(3),
            SigningCredentials = creds,
            Issuer = issuer,
            Audience = audience
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}