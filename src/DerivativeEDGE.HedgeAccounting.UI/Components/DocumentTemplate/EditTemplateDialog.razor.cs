using static DerivativeEDGE.HedgeAccounting.UI.Pages.DocumentTemplate.EditTemplate;

namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentTemplate;

public partial class EditTemplateDialog
{
    [Parameter]
    public EditTemplateModel EditTemplateForm { get; set; } = new();

    [Parameter]
    public long? SelectedClientId { get; set; }

    [Parameter]
    public EventCallback<EditTemplateModel> OnSave { get; set; }

    public DocumentTemplateActionResult DocumentTemplateActionResult { get; set; } = new DocumentTemplateActionResult(DocumentTemplateActions.Edit, true);

    [Inject]
    private IMediator MediatorService { get; set; } = default!;

    private ILocalStorageService LocalStorage { get; set; }
    private string HeaderText => EditTemplateForm.Id == Guid.Empty ? "New Template" : "Edit Template Details";

    private CustomValidation _customValidation;

    private bool Visibility { get; set; } = false;

    private bool IsMessageVisible { get; set; } = false;
    private bool IsEditMode { get; set; } = false;

    private static ErrorMessage CharacterCountValidation => new()
    {
        Title = "Some fixes are needed",
        Message = "Please update the highlighted fields to proceed."
    };

    private EditTemplateModel OldModel { get; set; }

    public void DlgRenameOpen(bool editMode = false)
    {
        Visibility = true;
        IsEditMode = editMode;
        IsMessageVisible = false;
        OldModel = EditTemplateForm.GetClone();
    }

    public async Task DlgRenameClose()
    {
        Visibility = false;
        EditTemplateForm = OldModel;
        await OnSave.InvokeAsync(EditTemplateForm);
    }


    private async Task SubmitInitialCreateForm(EditContext editContext)
    {
        var isValid = editContext.Validate();
        var updateObj =  (EditTemplateModel) editContext.Model;
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

            var documentTemplates = await MediatorService.Send(new ListDocumentTemplates.Query(SelectedClientId.GetValueOrDefault(0)));
            if (documentTemplates.DocumentTemplates
                .Any(c => string.Equals(c.Name, EditTemplateForm.Name?.TrimEnd(), StringComparison.OrdinalIgnoreCase) &&
                c.Id != EditTemplateForm.Id) &&
                DocumentTemplateActionResult.Action == DocumentTemplateActions.Edit)
            {
                errors.Add(nameof(Models.DocumentTemplate.Name),
                        ["Hedge Document Template name must be unique."]);
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
