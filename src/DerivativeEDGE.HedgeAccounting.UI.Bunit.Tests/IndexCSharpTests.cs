using DerivativeEdge.Blazor.ComponentLibrary.Services;
using DerivativeEDGE.HedgeAccounting.UI.Components;
using DerivativeEDGE.HedgeAccounting.UI.Handlers;
using DerivativeEDGE.HedgeAccounting.UI.Handlers.Handler.Queries;
using DerivativeEDGE.HedgeAccounting.UI.Mock;
using Syncfusion.Blazor;
using System.Threading.Tasks;

namespace DerivativeEDGE.HedgeAccounting.UI.Bunit.Tests;

public class IndexCSharpTests : TestContext
{
    [Fact]
    public async Task RenderComponent_AsAuthorizedUser_AbleToViewGrid()
    {
        //Arrange
        Services.AddSyncfusionBlazor();
        Services
            .AddSingleton<ITradeClient, TradeClient>()
            .AddSingleton<IAlertService, AlertService>()
            .AddAutoMapper(typeof(Program), typeof(GetTrades))
            .AddMediatR(cfg =>
             {
                 cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly, typeof(HandlersClassAnchor).Assembly);
             });
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("AuthorizedUser", AuthorizationState.Authorized);

        //Act
        var indexComponent = RenderComponent<ClientTrades>();
        await Task.Delay(5000);
        await VerifyXunit.Verifier.Verify(indexComponent.Markup);
    }
}