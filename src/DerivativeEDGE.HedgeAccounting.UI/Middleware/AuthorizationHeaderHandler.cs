namespace DerivativeEDGE.HedgeAccounting.UI.Middlware;

public class AuthorizationHeaderHandler : DelegatingHandler
{

    private readonly ApiTokenManager _apiTokenManager;

    public AuthorizationHeaderHandler(ApiTokenManager apiTokenManager)
    {
        _apiTokenManager = apiTokenManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiTokenManager.GetAccessToken());
        request.Headers.Add("Accept", "application/json");
        return await base.SendAsync(request, cancellationToken);
    }
}