using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Configuration;

public static class HttpClientRegistrationExtensions
{
    public static IServiceCollection AddHedgeAccountingHttpClients(this IServiceCollection services, IConfiguration configuration, ILogger? logger = null)
    {
        services.AddHttpClient<IHedgeAccountingApiClient, HedgeAccountingApiClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(configuration["HEDGE_ACCOUNTING_SERVICE_URL"]);
                    client.Timeout = new TimeSpan(0, 2, 30);
                }).ConfigurePrimaryHttpMessageHandler(serviceProvider => serviceProvider.GetRequiredService<JwtTokenForwardHandler>());

        // Read value from appsettings.json using the full section key
        var swapzillaBaseAddress = configuration["SWAPZILLA_BASE_ADDRESS"];

        if (string.IsNullOrWhiteSpace(swapzillaBaseAddress))
        {
            logger?.LogError("SWAPZILLA_BASE_ADDRESS is missing in configuration. HTTP clients will not be configured properly.");
            return services; // Return early without configuring HTTP clients
        }
                
        services.AddHttpClient("SwapzillaTrade", client =>
        {
            client.BaseAddress = new Uri(swapzillaBaseAddress);
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        });

        return services;
    }
}