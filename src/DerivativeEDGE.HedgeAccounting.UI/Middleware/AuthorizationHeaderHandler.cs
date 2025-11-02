namespace DerivativeEDGE.HedgeAccounting.UI.Middleware;

public class AuthorizationHeaderHandler(ApiTokenManager apiTokenManager) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiTokenManager.GetAccessToken());
        request.Headers.Add("Accept", "application/json");
        return await base.SendAsync(request, cancellationToken);
    }
}