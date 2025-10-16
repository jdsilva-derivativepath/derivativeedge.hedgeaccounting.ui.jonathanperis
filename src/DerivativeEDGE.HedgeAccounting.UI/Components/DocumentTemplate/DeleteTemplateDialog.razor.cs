namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentTemplate;

public partial class DeleteTemplateDialog
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
    public EventCallback<DocumentTemplateActionResult> OnDelete { get; set; }

    public Models.DocumentTemplate ToDelete { get; set; }

    private static ErrorMessage DeleteErrorMessage => new()
    {
        Title = "Failed to delete the Hedge Document Template",
        Message = "There was a system error deleting the Hedge Document Template. Please try again."
    };

    private async Task CancelButtonClicked()
    {
        await OnCancel.InvokeAsync();
    }

    public async Task DeleteButtonClicked()
    {
        try
        {
            Visible = false;
            SpinnerService.Show();
            var command = new DeleteDocumentTemplate.Command(ToDelete.Id);
            var response = await Mediator.Send(command);
            if (response.Success)
            {
                await AlertService.ShowToast("Hedge Document Template deleted.",
                    AlertKind.SuccessWithoutContent, true);
            }
            var actionResult = new DocumentTemplateActionResult(DocumentTemplateActions.Delete, response.Success)
            {
                ErrorMessage = !response.Success ? DeleteErrorMessage : null
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
