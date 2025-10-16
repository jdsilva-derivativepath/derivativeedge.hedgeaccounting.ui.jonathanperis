namespace DerivativeEDGE.HedgeAccounting.UI.Pages.DocumentTemplate;

public partial class EditTemplate
{
    private EditTemplateModel EditTemplateForm { get; set; } = new();

    private EditTemplateDialog _editTemplateDialogComponent;

    private ContentDialog _contentDialogComponent;

    [Parameter]
    public Guid? TemplateId { get; set; }

    private long ClientId { get; set; }

    private const int MAXCONTENT = 8;

    EditContext EditContext;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAlertService AlertService { get; set; } = default!;

    [Inject]
    private IMediator MediatorService { get; set; } = default!;

    [Inject]
    public SpinnerService SpinnerService { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; }

    private AllowedBehavior AllowedBehavior { get; set; } = default!;

    private ValidationMessageStore? _validationMessageStore;

    private bool IsErrorMessageVisible { get; set; }
    private bool IsEditEditContentDialogVisible { get; set; }

    private ErrorMessage ErrorMsg { get; set; }
    private bool IsActive { get; set; } = true;

    private bool IsPreviewMode { get; set; } = false;

    private string HtmlBody { get; set; }

    private static ErrorMessage SystemErrorMsg => new()
    {
        Title = "Failed to edit the Hedge Document Template",
        Message = "There was a system error editing the Hedge Document Template. Please try again."
    };

    private static ErrorMessage ContentEmptyValidationMsg => new()
    {
        Title = "Document content cannot be blank",
        Message = "Fill in the required Hedge Document Content."
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            SpinnerService.Show();
            var selectedClientId = await LocalStorageService!.GetItemAsync<long?>(StringConstants.HedgeDocumentSelectedClientId);

            await CheckAccessRights();

            if (selectedClientId is null || !AllowedBehavior.IsDpiUser)
            {
                ClientId = AllowedBehavior.ClientId;
            }
            else
            {
                ClientId = selectedClientId.GetValueOrDefault(0);
            }

            await GetTemplate();

            EditContext = new EditContext(EditTemplateForm);

            _validationMessageStore = new ValidationMessageStore(EditContext);
            // wire up an event handler to trigger validation events emitted by the Edit Context

            EditContext.OnValidationRequested += this.OnValidationRequested;
        }
        catch (Exception)
        {
            await AlertService.ShowToast("There was a problem retrieving template details.", AlertKind.Error,
                             "Error", showButton: true);
        }
        finally
        {
            SpinnerService.Hide();
        }

        await base.OnInitializedAsync();
    }

    private async Task GetTemplate()
    {
        var request = new GetDocumentContents.Query(ClientId);
        var result = await MediatorService.Send(request);

        if (result is not null && result.Data.Count != 0 || TemplateId is not null)
        {
            await GetTemplateDetails(result);
        }
    }

    private async Task CheckAccessRights()
    {
        try
        {
            var query = new GetAllowedBehavior.Query();
            var response = await MediatorService.Send(query, CancellationToken.None);

            AllowedBehavior = response.AllowedBehavior;
        }
        catch (Exception)
        {
            await AlertService.ShowToast("There was a problem retrieving the user's access rights.", AlertKind.Error,
                "Error", showButton: true);
        }
    }

    private async Task GetTemplateDetails(GetDocumentContents.Response currentClientContent)
    {
        var templateResult = await MediatorService.Send(new GetDocumentTemplateById.Query(TemplateId.GetValueOrDefault()));
        var request = new GetDocumentContents.Query(templateResult.DocumentTemplate.ClientId);
        var templateContentResult = await MediatorService.Send(request);

        if (templateResult is null || templateResult?.DocumentTemplate is null)
        {
            await AlertService.ShowToast("Template does not exist. You are now creating from a blank template.",
                AlertKind.Error, "Error", false);
            EditDefaultTemplateDetail(currentClientContent);
            return;

        }

        IsActive = templateResult.DocumentTemplate.Enabled;

        EditTemplateForm.HedgeDocumentContents = [.. templateContentResult.Data
        .Select(x =>
        {
            var templateDetail = templateResult.DocumentTemplate.HedgeDocumentTemplateDetails
                 .FirstOrDefault(cnt => cnt.HedgeDocumentContentId == x.Id);

            if (templateDetail is null)
            {
                return new HedgeDocumentContentViewModel()
                {
                    Id = Guid.NewGuid(),
                    HedgeDocumentContentId = x.Id,
                    Order = x.Order,
                    Hidden = templateDetail is null ? !x.Required : templateDetail.Hidden,
                    HtmlBody = templateDetail?.HtmlBody,
                    Name = x.Name,
                    Required = x.Required
                };
            }
            else
            {
                return new HedgeDocumentContentViewModel()
                {
                    Id = templateDetail.Id,
                    HedgeDocumentContentId = templateDetail.HedgeDocumentContentId,
                    Order = x.Order,
                    Hidden = templateDetail.Hidden,
                    HtmlBody = templateDetail?.HtmlBody,
                    Name = x.Name,
                    Required = x.Required
                };
            }

        })];

        EditTemplateForm.Id = templateResult.DocumentTemplate.Id;
        EditTemplateForm.Name = templateResult.DocumentTemplate.Name;
        EditTemplateForm.Description = templateResult.DocumentTemplate.Description;
    }

    private void EditDefaultTemplateDetail(GetDocumentContents.Response result)
    {
        EditTemplateForm.HedgeDocumentContents = [.. result.Data.Select(x => new HedgeDocumentContentViewModel()
        {
            Id = Guid.NewGuid(),
            HedgeDocumentContentId = x.Id,
            Order = x.Order,
            Hidden = !x.Required,
            HtmlBody = "",
            Name = x.Name,
            Required = x.Required
        })];
    }

    private async Task FormSubmit()
    {
        var isValid = EditContext.Validate();
        var model = (EditTemplateModel)EditContext.Model;

        IsErrorMessageVisible = !isValid;

        if (model.HedgeDocumentContents.Count == 0)
        {
            await AlertService.ShowToast("There was a system error creating the Hedge Document Template. Please try again.",
                AlertKind.Error, "Failed to edit the Hedge Document Template", false);
            return;
        }

        if (IsErrorMessageVisible)
        {
            ErrorMsg = ContentEmptyValidationMsg;
            return;
        }
        try
        {
            // Edit Save
            var request = new UpdateDocumentTemplate.Command(EditTemplateForm.Id, EditTemplateForm.Name, EditTemplateForm.Description, IsActive, AllowedBehavior.UserId,
                [.. EditTemplateForm.HedgeDocumentContents.Select(c => c.ToRequestModel())]);

            var result = await MediatorService.Send(request);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                await AlertService.ShowToast("Hedge Document Template edited.", AlertKind.Success, "Success", false);
                await Task.Delay(3000);
                EditContext.MarkAsUnmodified();
                NavigationManager.NavigateTo("hrhedgedocumenttemplate");
            }
        }
        catch (Exception ex)
        {
            IsErrorMessageVisible = true;
            ErrorMsg = SystemErrorMsg;
        }
    }

    private async Task UpdatePage(DocumentContentActionResult result)
    {
        IsEditEditContentDialogVisible = false;

        if (result.Success)
        {
            await GetTemplate();
            return;
        }

        ShowActionFailed(result.ErrorMessage);
    }

    private void UpdateTemplateName(EditTemplateModel createDocumentTemplate)
    {
        EditTemplateForm = createDocumentTemplate;
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        EditTemplateForm.Validate(_validationMessageStore);
        EditContext.NotifyValidationStateChanged();
    }

    public void OnToggleClick(HedgeDocumentContentViewModel model)
    {
        var updateObj = EditTemplateForm.HedgeDocumentContents
                    .FirstOrDefault(x => x.Id == model.Id);

        if (updateObj != null)
        {
            updateObj.Hidden = !model.Hidden;
            EditTemplateForm.HedgeDocumentContents.Remove(model);
            EditTemplateForm.HedgeDocumentContents.Add(updateObj);
        }

        StateHasChanged();
    }

    private void OnCancelClick()
    {
        NavigationManager.NavigateTo("hrhedgedocumenttemplate");
    }

    private void DlgRenameOpen()
    {
        _editTemplateDialogComponent.DocumentTemplateActionResult = new DocumentTemplateActionResult(DocumentTemplateActions.Edit, false);
        _editTemplateDialogComponent.DlgRenameOpen(true);
    }

    private void CancelEditEdit()
    {
        IsEditEditContentDialogVisible = false;
    }

    private void ShowActionFailed(ErrorMessage errorMessage)
    {
        IsErrorMessageVisible = true;
        ErrorMsg = errorMessage;
    }

    private async Task EnablePreviewMode()
    {
        var contentRequest = new GetDocumentContents.Query(ClientId);
        var contents = await MediatorService.Send(contentRequest);

        var documentTemplateDetails = EditTemplateForm.HedgeDocumentContents
            .Where(x => !x.Hidden && !string.IsNullOrWhiteSpace(x.HtmlBody))
            .OrderBy(x => x.Order)
            .Select(c => c.ToRequestModel())
            .ToList();

        HtmlBody = string.Join("", documentTemplateDetails.Select(detail =>
        {
            var associatedHeader = contents.Data
                .First(x => x.Id == detail.HedgeDocumentContentId);

            return $"<h3 class='dp-preview-content-header'>{associatedHeader.Name}</h3><div class='dp-preview-content-body'>{detail.HtmlBody}<div/>";
        }));

        IsPreviewMode = true;
    }

    private void DisablePreviewMode()
    {
        HtmlBody = string.Empty;

        IsPreviewMode = false;
    }

    public class ShouldNotEmptyWhenRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (HedgeDocumentContentViewModel)validationContext.ObjectInstance;
            if (!model.Required && model.Hidden)
            {
                return ValidationResult.Success;
            }

            var htmlBody = value as string;
            return string.IsNullOrWhiteSpace(htmlBody) || Regex.IsMatch(model.HtmlBody, @"^\s*(<p>\s*(<br\s*/?>\s*)*</p>|<br\s*/?>)\s*$", RegexOptions.IgnoreCase)
                ? new ValidationResult("Content is required")
                : ValidationResult.Success;
        }
    }

    public class EditTemplateModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Enter the Template Name.")]
        [MaxLength(50, ErrorMessage = "Template name maximum character is up to 50.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Enter the Description.")]
        [MaxLength(150, ErrorMessage = "Template name maximum character is up to 150.")]
        public string Description { get; set; } = "";

        [ValidateComplexType]
        public List<HedgeDocumentContentViewModel> HedgeDocumentContents { get; set; } = [];

        public void Validate(ValidationMessageStore? validationStore)
        {
            if (validationStore is null)
            {
                return;
            }

            // clear our section of the message store
            validationStore.Clear();

            foreach (var item in HedgeDocumentContents ?? [])
            {
                // add any new messages using a FieldIdentifier instance to specify the object instance and the property name 
                if ((string.IsNullOrEmpty(item.HtmlBody) || Regex.IsMatch(item.HtmlBody, @"^\s*(<p>\s*(<br\s*/?>\s*)*</p>|<br\s*/?>)\s*$", RegexOptions.IgnoreCase)) && (item.Required || !item.Hidden))
                {
                    validationStore.Add(new FieldIdentifier(item, nameof(item.HtmlBody)), $"Content is required");
                }
            }
        }

        public EditTemplateModel GetClone()
        {
            return (EditTemplateModel)MemberwiseClone();
        }
    }
}