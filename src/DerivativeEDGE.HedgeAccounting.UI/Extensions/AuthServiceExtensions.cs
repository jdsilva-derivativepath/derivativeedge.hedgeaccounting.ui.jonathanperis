using IdentityModel.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddOAuth2(this IServiceCollection services, IConfiguration configuration)
    {
        var parameters = new Dictionary<string, string>
        {
            { "ClientId", configuration["HEDGE_ACCOUNTING_SERVICE_CLIENTID"] },
            { "ClientSecret", configuration["HEDGE_ACCOUNTING_SERVICE_CLIENTSECRET"] }
        };

        services.AddAccessTokenManagement(options =>
        {
            options.Client.Clients.Add("identityserver", new ClientCredentialsTokenRequest
            {
                Address = configuration["AUTH_SERVICE_URL"],
                AuthorizationHeaderStyle = BasicAuthenticationHeaderStyle.Rfc6749,
                ClientCredentialStyle = ClientCredentialStyle.PostBody,
                ClientId = configuration["HEDGE_ACCOUNTING_SERVICE_CLIENTID"]!,
                ClientSecret = configuration["HEDGE_ACCOUNTING_SERVICE_CLIENTSECRET"],
                Parameters = Parameters.FromObject(parameters)
            });
        })
        .ConfigureBackchannelHttpClient(httpClient =>
        {
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });
        return services;
    }
}