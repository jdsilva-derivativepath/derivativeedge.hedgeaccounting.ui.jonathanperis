using static DerivativeEDGE.HedgeAccounting.UI.Pages.HedgeRelationship.CreateHrDocument;

namespace DerivativeEDGE.HedgeAccounting.UI.Components.HedgeRelationship;

public partial class CreateHrDocumentDialog
{
    [Parameter]
    public CreateHrDocumentModel CreateHrDocumentForm { get; set; } = new();

    [Parameter]
    public long? HedgeRelationshipId { get; set; }

    [Parameter]
    public EventCallback<CreateHrDocumentModel> OnSave { get; set; }

    public HrDocumentActionResult HrDocumentActionResult { get; set; } = new HrDocumentActionResult(HrDocumentActions.Create, true);

    [Inject]
    private IMediator MediatorService { get; set; } = default!;

    private string HeaderText => CreateHrDocumentForm.Id == Guid.Empty ? "New Hedge Document" : "Edit Document Details";

    private CustomValidation _customValidation;

    private bool Visibility { get; set; } = false;

    private bool IsMessageVisible { get; set; } = false;
    private bool IsEditMode { get; set; } = false;

    private static ErrorMessage CharacterCountValidation => new()
    {
        Title = "Some fixes are needed",
        Message = "Please update the highlighted fields to proceed."
    };

    private CreateHrDocumentModel OldModel { get; set; }
    public void DlgRenameOpen(bool editMode = false)
    {
        Visibility = true;
        IsEditMode = editMode;
        IsMessageVisible = false;
        OldModel = CreateHrDocumentForm.GetClone();
 
    }

    public async Task DlgRenameClose()
    {
        Visibility = false;
        CreateHrDocumentForm = OldModel;
        await OnSave.InvokeAsync(CreateHrDocumentForm);
    }

    private async Task SubmitInitialCreateForm(EditContext editContext)
    {
        var isValid = editContext.Validate();
        var updateObj =  (CreateHrDocumentModel) editContext.Model;
        IsMessageVisible = !(isValid && await CustomValidation());

        if (IsMessageVisible)
        {
            return;
        }

        Visibility = false;
        await OnSave.InvokeAsync(updateObj);
        StateHasChanged();
    }

    private async Task<bool> CustomValidation()
    {
        try
        {
            _customValidation?.ClearErrors();

            var errors = new Dictionary<string, List<string>>();

            var documentContents = await MediatorService.Send(new ListHrDocuments.Query(HedgeRelationshipId.GetValueOrDefault(0)));
            if (documentContents.RelationshipDocumentContents
                .Any(c => string.Equals(c.DocumentName, CreateHrDocumentForm.Name?.TrimEnd(), StringComparison.OrdinalIgnoreCase)) &&
                HrDocumentActionResult.Action == HrDocumentActions.Create)
            {
                errors.Add(nameof(Models.DocumentTemplate.Name),
                        ["Document name must be unique."]);
            }

            if (errors.Count != 0)
            {
                _customValidation?.DisplayErrors(errors);
                return false;
            }
            return true;
        }
        catch (Exception)
        {
            Visibility = false;
            
            return false;
        }
    }
}
