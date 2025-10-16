using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DerivativeEDGE.HedgeAccounting.UI.Services.HedgeAccountingApi;

public class HedgeAccountingApiService : IHedgeAccountingApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HedgeAccountingApiService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HedgeAccountingApiService(HttpClient httpClient, ILogger<HedgeAccountingApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
    }

    public async Task<TResponse> GetAsync<TResponse>(string url)
    {
        return await GetAsync<TResponse>(url, HedgeAccountingApiVersions.v1);
    }

    public async Task<TResponse> GetAsync<TResponse>(string url, HedgeAccountingApiVersions version)
    {
        try
        {
            var requestUri = version == HedgeAccountingApiVersions.None ? url : $"api/{version}/{url}";

            using var httpResponse = await _httpClient.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                return JsonSerializer.Deserialize<TResponse>(responseBody, _jsonSerializerOptions);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, message: StringConstants.APICallFailed, url);
        }
        return default;
    }

    public async Task<TResponse> GetAsync<TRequest, TResponse>(string url, TRequest request,
        CancellationToken cancellationToken)
    {
        return await SendAsync<TRequest, TResponse>(HttpMethod.Get, url, request,
            HedgeAccountingApiVersions.v1, cancellationToken);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request,
        CancellationToken cancellationToken)
    {
        return await SendAsync<TRequest, TResponse>(HttpMethod.Post, url, request,
            HedgeAccountingApiVersions.v1, cancellationToken);
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string url, TRequest request,
        CancellationToken cancellationToken)
    {
        return await SendAsync<TRequest, TResponse>(HttpMethod.Patch, url, request,
            HedgeAccountingApiVersions.v1, cancellationToken);
    }

    public async Task<TResponse> DeleteAsync<TRequest, TResponse>(string url, TRequest request,
        CancellationToken cancellationToken)
    {
        return await SendAsync<TRequest, TResponse>(HttpMethod.Delete, url, request,
            HedgeAccountingApiVersions.v1, cancellationToken);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest request,
        HedgeAccountingApiVersions version, CancellationToken cancellationToken)
    {
        try
        {
            using var httpResponse = await _httpClient.SendAsync(
                new HttpRequestMessage
                {
                    Method = method,
                    RequestUri = new Uri($"{_httpClient.BaseAddress}api/{version}/{url}"),
                    Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                },
                cancellationToken);

            httpResponse.EnsureSuccessStatusCode();
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                return JsonSerializer.Deserialize<TResponse>(responseBody, _jsonSerializerOptions);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, message: StringConstants.APICallFailed, url);
        }
        return default;
    }
}
