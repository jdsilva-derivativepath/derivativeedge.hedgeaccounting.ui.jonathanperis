using static DerivativeEDGE.HedgeAccounting.UI.Pages.HedgeRelationship.EditHrDocument;

namespace DerivativeEDGE.HedgeAccounting.UI.Components.HedgeRelationship;

public partial class EditHrDocumentDialog
{
    [Parameter]
    public EditHrDocumentModel EditHrDocumentForm { get; set; } = new();

    [Parameter]
    public long? SelectedClientId { get; set; }

    [Parameter]
    public EventCallback<EditHrDocumentModel> OnSave { get; set; }
    public HrDocumentActionResult HrDocumentActionResult { get; set; } = new HrDocumentActionResult(HrDocumentActions.Edit, true);

    private string HeaderText => EditHrDocumentForm.Id == Guid.Empty ? "New Document" : "Edit Document Details";

    private CustomValidation _customValidation;

    private bool Visibility { get; set; } = false;

    private bool IsMessageVisible { get; set; } = false;
    private bool IsEditMode { get; set; } = false;

    private static ErrorMessage CharacterCountValidation => new()
    {
        Title = "Some fixes are needed",
        Message = "Please update the highlighted fields to proceed."
    };

    private EditHrDocumentModel OldModel { get; set; }

    public void DlgRenameOpen(bool editMode = false)
    {
        Visibility = true;
        IsEditMode = editMode;
        IsMessageVisible = false;
        OldModel = EditHrDocumentForm.GetClone();
    }

    public async Task DlgRenameClose()
    {
        Visibility = false;
        EditHrDocumentForm = OldModel;
        await OnSave.InvokeAsync(EditHrDocumentForm);
    }

    private async Task SubmitInitialCreateForm(EditContext editContext)
    {
        var updateObj =  (EditHrDocumentModel) editContext.Model;

        Visibility = false;
        await OnSave.InvokeAsync(updateObj);
        StateHasChanged();
    }
}
