using M2mCredentials = DerivativeEDGE.HedgeAccounting.UI.Models.M2mCredentials;

namespace DerivativeEDGE.HedgeAccounting.UI.Services.HedgeAccountingApi;

public class ApiTokenManager
{
    private readonly ApiTokenProvider _tokenProvider;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiTokenManager> _logger;

    public ApiTokenManager(
        ApiTokenProvider tokenProvider,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<ApiTokenManager> logger)
    {
        _tokenProvider = tokenProvider;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string GetAccessToken()
    {
        if (_tokenProvider.AccessToken == null || DateTime.Now >= _tokenProvider.ExpiresAt.AddSeconds(-60))
        {
            try
            {
                var endpoint = new Uri($"{_configuration[ConfigurationKeys.AuthServiceUrl]}authorize");
                var jsonData = JsonConvert.SerializeObject(new M2mCredentials(
                        _configuration[ConfigurationKeys.HedgeAccountingServiceClientId] ?? string.Empty,
                        _configuration[ConfigurationKeys.HedgeAccountingServiceClientSecret] ?? string.Empty));

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                using var httpClient = _httpClientFactory.CreateClient();
                using var httpResponse = httpClient.PostAsync(endpoint, content).GetAwaiter().GetResult();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ApiToken>(httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    _tokenProvider.AccessToken = result?.access_token ?? string.Empty;
                    _tokenProvider.ExpiresAt = DateTime.Now.AddSeconds(result?.expires_in ?? 0);
                    _tokenProvider.TokenType = result?.token_type ?? string.Empty;
                    _logger.LogInformation("Success requesting Hedge Accounting API M2M token.");
                }
                else
                {
                    _logger.LogInformation("Failed requesting Hedge Accounting API M2M token.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in requesting Hedge Accounting API M2M token.");
            }
        }
        else
        {
            _logger.LogInformation("Use cached token.");
        }
        return _tokenProvider?.AccessToken ?? string.Empty;
    }
}
