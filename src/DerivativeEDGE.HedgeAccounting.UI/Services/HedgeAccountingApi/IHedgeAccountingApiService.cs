namespace DerivativeEDGE.HedgeAccounting.UI.Services.HedgeAccountingApi;

public interface IHedgeAccountingApiService
{
    Task<TResponse> GetAsync<TResponse>(string url);
    Task<TResponse> GetAsync<TResponse>(string url, HedgeAccountingApiVersions version);
    Task<TResponse> GetAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken);
    Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken);
    Task<TResponse> PatchAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken);
    Task<TResponse> DeleteAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken);
    Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest request,
        HedgeAccountingApiVersions version, CancellationToken cancellationToken);
}
