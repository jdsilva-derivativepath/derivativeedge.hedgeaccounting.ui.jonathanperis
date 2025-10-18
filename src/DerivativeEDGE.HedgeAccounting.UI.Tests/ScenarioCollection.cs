namespace DerivativeEDGE.HedgeAccounting.UI.Tests;


[CollectionDefinition("scenarios")]
public class ScenarioCollection : ICollectionFixture<BlazorServerWebApplicationFactory>
{

}

[Collection("scenarios")]
public abstract class ScenarioContext(BlazorServerWebApplicationFactory blazorServerWebApplicationFactory)
{
    protected readonly string _serverAddress = blazorServerWebApplicationFactory.ServerAddress;
    protected readonly BlazorServerWebApplicationFactory _blazorServerWebApplicationFactory = blazorServerWebApplicationFactory;
}
