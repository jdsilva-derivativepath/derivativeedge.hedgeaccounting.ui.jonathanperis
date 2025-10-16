namespace DerivativeEDGE.HedgeAccounting.UI.Tests;


[CollectionDefinition("scenarios")]
public class ScenarioCollection : ICollectionFixture<BlazorServerWebApplicationFactory>
{

}

[Collection("scenarios")]
public abstract class ScenarioContext
{
    protected readonly string _serverAddress;
    protected readonly BlazorServerWebApplicationFactory _blazorServerWebApplicationFactory;
    protected ScenarioContext(BlazorServerWebApplicationFactory blazorServerWebApplicationFactory)
    {
        _blazorServerWebApplicationFactory = blazorServerWebApplicationFactory;
        _serverAddress = blazorServerWebApplicationFactory.ServerAddress;
    }
}
