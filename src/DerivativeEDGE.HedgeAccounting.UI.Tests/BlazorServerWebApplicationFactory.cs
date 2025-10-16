using Bogus;
using DerivativeEDGE.HedgeAccounting.UI.Mock;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DerivativeEDGE.HedgeAccounting.UI.Tests;

/*
 Reference : https://danieldonbavand.com/2022/06/13/using-playwright-with-the-webapplicationfactory-to-test-a-blazor-application/
*/

public class BlazorServerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IHost? _host;
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();

        //Arrange test data
        var clientId = 1000;
        var trades = new Faker<Trade>()
            .UseSeed(100) // ensures the same consistent test data
            .RuleFor(t => t.TradeStatus, s => s.Random.Bool())
            .RuleFor(t => t.ClientName, s => s.Company.CompanyName())
            .RuleFor(t => t.Amount, a => a.Random.Decimal(-100000, 100000))
            .RuleFor(t => t.Notional, a => a.Random.Decimal(100, 100000))
            .RuleFor(t => t.Currency, s => s.PickRandom("USD", "PHP", "EUR", "CNH", "CHF"))
            .RuleFor(t => t.DealDate, d => d.PickRandom("06/05/2023", "06/26/2023", "06/28/2023", "06/24/2023", "06/10/2023"))
            .RuleFor(t => t.TransactionDate, d => d.PickRandom(new DateTime(2023, 06, 05, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 09, 05, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 10, 05, 0, 0, 0, DateTimeKind.Utc)))
            .RuleFor(t => t.TradeDate, d => d.PickRandom(new DateTime(2023, 06, 05, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 09, 05, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 10, 05, 0, 0, 0, DateTimeKind.Utc)))
            .RuleFor(t => t.ClientId, id => clientId++ * 5)
            .RuleFor(t => t.Description, "This is description for the trade.")
            .Generate(10);

        var tradeService = A.Fake<ITradeClient>();
        A.CallTo(() => tradeService.GetTrades()).Returns(trades);
        // Modify the host builder to use Kestrel instead
        // of TestServer so we can listen on a real address.

        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();

            webHostBuilder.ConfigureTestServices(services =>
            {
                services.AddSingleton(tradeService);

            });


            webHostBuilder.UseSetting("PROXY_MODE", "LocalDevelopment");

        });

        // Create and start the Kestrel server before the test server,
        // otherwise due to the way the deferred host builder works
        // for minimal hosting, the server will not get "initialized
        // enough" for the address it is listening on to be available.
        // See https://github.com/dotnet/aspnetcore/issues/33846


        _host = builder.Build();

        _host.Start();

        // Extract the selected dynamic port out of the Kestrel server
        // and assign it onto the client options for convenience so it
        // "just works" as otherwise it'll be the default http://localhost
        // URL, which won't route to the Kestrel-hosted HTTP server.

        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();
        ClientOptions.AllowAutoRedirect = false;

        // Return the host that uses TestServer, rather than the real one.
        // Otherwise the internals will complain about the host's server
        // not being an instance of the concrete type TestServer.
        // See https://github.com/dotnet/aspnetcore/pull/34702.

        testHost.Start();
        return testHost;
    }


    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    private void EnsureServer()
    {
        if (_host is null)
        {
            // This forces WebApplicationFactory to bootstrap the server
            using var _ = CreateDefaultClient();
        }
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        _host?.Dispose();

        return Task.CompletedTask;
    }
}
