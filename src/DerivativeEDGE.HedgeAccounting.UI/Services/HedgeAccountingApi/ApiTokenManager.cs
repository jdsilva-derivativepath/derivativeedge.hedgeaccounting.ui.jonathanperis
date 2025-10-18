using M2mCredentials = DerivativeEDGE.HedgeAccounting.UI.Models.M2mCredentials;

namespace DerivativeEDGE.HedgeAccounting.UI.Services.HedgeAccountingApi;

public class ApiTokenManager(
    ApiTokenProvider tokenProvider,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<ApiTokenManager> logger)
{
    public string GetAccessToken()
    {
        if (tokenProvider.AccessToken == null || DateTime.Now >= tokenProvider.ExpiresAt.AddSeconds(-60))
        {
            try
            {
                var endpoint = new Uri($"{configuration[ConfigurationKeys.AuthServiceUrl]}authorize");
                var jsonData = JsonConvert.SerializeObject(new M2mCredentials(
                        configuration[ConfigurationKeys.HedgeAccountingServiceClientId] ?? string.Empty,
                        configuration[ConfigurationKeys.HedgeAccountingServiceClientSecret] ?? string.Empty));

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                using var httpClient = httpClientFactory.CreateClient();
                using var httpResponse = httpClient.PostAsync(endpoint, content).GetAwaiter().GetResult();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ApiToken>(httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    tokenProvider.AccessToken = result?.access_token ?? string.Empty;
                    tokenProvider.ExpiresAt = DateTime.Now.AddSeconds(result?.expires_in ?? 0);
                    tokenProvider.TokenType = result?.token_type ?? string.Empty;
                    logger.LogInformation("Success requesting Hedge Accounting API M2M token.");
                }
                else
                {
                    logger.LogInformation("Failed requesting Hedge Accounting API M2M token.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in requesting Hedge Accounting API M2M token.");
            }
        }
        else
        {
            logger.LogInformation("Use cached token.");
        }
        return tokenProvider?.AccessToken ?? string.Empty;
    }
}
