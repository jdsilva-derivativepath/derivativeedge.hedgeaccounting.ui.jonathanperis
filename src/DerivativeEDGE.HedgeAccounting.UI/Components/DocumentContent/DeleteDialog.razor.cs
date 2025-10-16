namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentContent;

public partial class DeleteDialog
{
    [Inject]
    public SpinnerService SpinnerService { get; set; }

    [Inject]
    public IMediator Mediator { get; set; }

    [Inject]
    public IAlertService AlertService { get; set; }

    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback<DocumentContentActionResult> OnDelete { get; set; }

    public Models.DocumentContent ToDelete { get; set; }

    private static ErrorMessage DeleteErrorMessage => new()
    {
        Title = "Failed to delete the Hedge Document Content",
        Message = "There was a system error deleting the Hedge Document Content. Please try again."
    };

    private async Task CancelButtonClicked()
    {
        ToDelete = null;
        await OnCancel.InvokeAsync();
    }

    public async Task DeleteButtonClicked()
    {
        try
        {
            Visible = false;
            SpinnerService.Show();
            var command = new DeleteDocumentContent.Command(ToDelete.Id);
            var response = await Mediator.Send(command);
            var success = !string.IsNullOrEmpty(response.Message);
            if (success)
            {
                await AlertService.ShowToast("Hedge Document Content deleted.",
                    AlertKind.SuccessWithoutContent, true);
            }
            var actionResult = new DocumentContentActionResult(DocumentContentActions.Delete, success)
            {
                ErrorMessage = !success ? DeleteErrorMessage : null
            };
            await OnDelete.InvokeAsync(actionResult);
        }
        catch (Exception)
        {
            await OnDelete.InvokeAsync(new(DocumentContentActions.Delete, false)
            {
                ErrorMessage = DeleteErrorMessage
            });
        }
        finally
        {
            SpinnerService.Hide();
            ToDelete = null;
        }
    }
}
