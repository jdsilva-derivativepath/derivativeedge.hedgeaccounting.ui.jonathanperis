namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentContent;

public partial class ContentDialog
{
    private CustomValidation _customValidation;

    [Inject]
    public IMediator Mediator { get; set; }

    [Inject]
    public IAlertService AlertService { get; set; }

    [Inject]
    public SpinnerService SpinnerService { get; set; }

    [Parameter]
    public AllowedBehavior AllowedBehavior { get; set; }

    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public long ClientId { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback<DocumentContentActionResult> OnSave { get; set; }

    public Models.DocumentContent Model { get; set; } = new();

    private bool IsMessageVisible { get; set; } = false;

    private string HeaderText => Model.Id == Guid.Empty ? "New Document Content" : "Edit Document Content";

    private static ErrorMessage SaveNewErrorMessage => new()
    {
        Title = "Failed to create the Hedge Document Content",
        Message = "There was a system error creating the Hedge Document Content. Please try again."
    };

    private static ErrorMessage SaveChangesErrorMessage => new()
    {
        Title = "Failed to update the Hedge Document Content",
        Message = "There was a system error updating the Hedge Document Content. Please try again."
    };

    private async Task FormSubmit(EditContext context)
    {
        var isValid = context.Validate();

        IsMessageVisible = !(isValid && await CustomValidation());
        
        if (IsMessageVisible)
        {
            return;
        }

        if (Model.Id == Guid.Empty)
        {
            await SaveNew();
            return;
        }
        await SaveChanges();
    }

    private async Task<bool> CustomValidation()
    {
        try
        {
            _customValidation?.ClearErrors();

            var errors = new Dictionary<string, List<string>>();
            var response = await Mediator.Send(new GetDocumentContents.Query(ClientId));
            if (response.Data.Any(c => string.Equals(c.Name, Model.Name?.TrimEnd(), StringComparison.OrdinalIgnoreCase) 
                && c.Id != Model.Id))
            {
                errors.Add(nameof(Models.DocumentContent.Name),
                    ["Hedge Document Content Name should be unique."]);
            }

            if (errors.Count != 0)
            {
                _customValidation?.DisplayErrors(errors);
                return false;
            }
            return true;
        }
        catch(Exception)
        {
            Visible = false;
            var actionResult = new DocumentContentActionResult(Model.Id == Guid.Empty ? 
                DocumentContentActions.Create : DocumentContentActions.Edit, false)
            {
                ErrorMessage = Model.Id == Guid.Empty ? SaveNewErrorMessage : SaveChangesErrorMessage
            };
            await OnSave.InvokeAsync(actionResult);
            Model = new();
            return false;
        }
    }

    private async Task CancelButtonClicked()
    {
        await OnCancel.InvokeAsync();
        Model = new();
    }

    private async Task SaveNew()
    {
        try
        {
            Visible = false;
            SpinnerService.Show();
            var command = new CreateDocumentContent.Command(Model.Name?.TrimEnd(), ClientId, AllowedBehavior.UserId, Model.Required);
            var response = await Mediator.Send(command);
            var success = !string.IsNullOrEmpty(response?.Message);
            if (success)
            {
                await AlertService.ShowToast("Start using your Hedge Document Content when creating your Hedge Document Templates.",
                    AlertKind.SuccessWithConent, "Hedge Document Content created.", true);
            }

            var actionResult = new DocumentContentActionResult(DocumentContentActions.Create, success)
            {
                ErrorMessage = !success ? SaveNewErrorMessage : null
            };
            await OnSave.InvokeAsync(actionResult);
        }
        catch (Exception)
        {
            await OnSave.InvokeAsync(new(DocumentContentActions.Create, false)
            {
                ErrorMessage = SaveNewErrorMessage
            });
        }
        finally
        {
            SpinnerService.Hide();
            Model = new();
        }
    }

    private async Task SaveChanges()
    {
        try
        {
            Visible = false;
            SpinnerService.Show();
            var command = new UpdateDocumentContent.Command(Model.Id, Model.Name?.TrimEnd(), ClientId,
                Model.Required);
            var response = await Mediator.Send(command);
            var success = !string.IsNullOrEmpty(response?.Message);
            if (success)
            {
                var message = AllowedBehavior.IsDpiUser ?
                    "Hedge Document Content changes will apply to all existing Hedge Document Templates associated with the client." :
                    "Hedge Document Content changes will apply to all of your existing Hedge Document Templates.";
                await AlertService.ShowToast(message,
                    AlertKind.SuccessWithConent, "Hedge Document Content updated.", true);
            }
            var actionResult = new DocumentContentActionResult(DocumentContentActions.Edit, success)
            {
                ErrorMessage = !success ? SaveChangesErrorMessage : null
            };
            await OnSave.InvokeAsync(actionResult);
        }
        catch (Exception)
        {
            await OnSave.InvokeAsync(new(DocumentContentActions.Edit, false)
            {
                ErrorMessage = SaveChangesErrorMessage
            });
        }
        finally
        {
            SpinnerService.Hide();
            Model = new();
        }
    }

}
