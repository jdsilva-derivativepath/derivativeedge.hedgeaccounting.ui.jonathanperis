namespace DerivativeEDGE.HedgeAccounting.UI.Pages;

public partial class TestConnection
{
    [Inject] 
    private IAlertService Alert { get; set; }
    [Inject]
    private IMediator Mediator { get; set; }

    private string apiResponse = string.Empty;
    private async Task TriggerApi()
    {
        try
        {
            var request = new Handlers.TestHedgeAccountingApiConnection.Request("Test");
            var response = await Mediator.Send(request, CancellationToken.None);
            apiResponse = response.ResponseData;
            await Alert.ShowToast(response.ResponseData, AlertKind.Success, "Success", showButton: true);
        }
        catch (Exception ex)
        {
            await Alert.ShowToast($"Error connecting to Hedge Accounting API.: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }
}
