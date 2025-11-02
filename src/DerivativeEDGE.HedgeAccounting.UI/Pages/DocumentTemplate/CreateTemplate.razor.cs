namespace DerivativeEDGE.HedgeAccounting.UI.Pages.DocumentTemplate;

public partial class CreateTemplate
{
    private CreateTemplateModel CreateTemplateForm { get; set; } = new();

    private CreateTemplateDialog _createTemplateDialogComponent;

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
    private SpinnerService SpinnerService { get; set; }

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; }

    private AllowedBehavior AllowedBehavior { get; set; } = default!;

    private ValidationMessageStore? _validationMessageStore;

    private bool IsErrorMessageVisible { get; set; }
    private bool IsCreateEditContentDialogVisible { get; set; }
    private bool IsCloneFromDPI { get; set; }
    private bool IsUpdatedContentEditor { get; set; } = false;

    private bool IsActive { get; set; } = true;

    private ErrorMessage ErrorMsg { get; set; }

    private string PageTitle { get; set; }

    private string CreationMode { get; set; } = DocumentTemplateActions.Create;

    private bool IsPreviewMode { get; set; }

    private string HtmlBody { get; set; }

    private bool CreateTemplateNameVisibility { get; set; } = true;

    private bool IsEditMode { get; set; } = false;

    private static ErrorMessage SystemErrorMsg => new()
    {
        Title = "Failed to create the Hedge Document Template",
        Message = "There was a system error creating the Hedge Document Template. Please try again."
    };

    private static ErrorMessage ContentEmptyValidationMsg => new()
    {
        Title = "Document content cannot be blank",
        Message = "Fill in the required Hedge Document Content."
    };

    private async Task GetCreationMode()
    {
        CreationMode = await LocalStorageService!.GetItemAsync<string>(StringConstants.HedgeDocumentTemplateMode);
    }

    protected override async Task OnInitializedAsync()
    {
        // Initialize Contents
        try
        {
            SpinnerService.Show();
            var selectedClientId = await LocalStorageService!.GetItemAsync<long?>(StringConstants.HedgeDocumentSelectedClientId);
            
            await CheckAccessRights();

            if (selectedClientId is null || (AllowedBehavior is not null && !AllowedBehavior.IsDpiUser))
            {
                ClientId = AllowedBehavior.ClientId;
            }
            else
            {
                ClientId = selectedClientId.GetValueOrDefault(0);
            }
            
            await GetTemplate();
            await GetCreationMode();

            PageTitle = CreationMode == DocumentTemplateActions.Duplicate ? "Duplicate Template" : "Create Template";
            EditContext = new EditContext(CreateTemplateForm);
            
            _validationMessageStore = new ValidationMessageStore(EditContext);
            
            EditContext.OnValidationRequested += OnValidationRequested;
        }
        catch (Exception ex)
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

        await GetTemplateDetails(result);
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
        if (TemplateId is not null)
        {
            var templateResult = await MediatorService.Send(new GetDocumentTemplateById.Query(TemplateId.GetValueOrDefault()));
            var request = new GetDocumentContents.Query(templateResult.DocumentTemplate.ClientId);
            var templateContentResult = await MediatorService.Send(request);
            var templates = currentClientContent.Data.Count != 0 ? currentClientContent.Data : templateContentResult.Data;

            if (templateResult is null || templateResult?.DocumentTemplate is null)
            {
                await AlertService.ShowToast("Template does not exist. You are now creating from a blank template.",
                    AlertKind.Error, "Error", false);
                CreateDefaultTemplateDetail(currentClientContent);
                return;

            }

            // Client has a content - Copy only the html body
            if (templates.Count != 0 && currentClientContent.Data.Count != 0)
            {
                var idx = 0;
                CreateTemplateForm.HedgeDocumentContents = [.. templates.Select(x =>
                {
                    var content = idx < templateResult.DocumentTemplate.HedgeDocumentTemplateDetails.Count ?
                        templateResult.DocumentTemplate.HedgeDocumentTemplateDetails[idx] 
                        : null;
                    idx++;

                    return new HedgeDocumentContentViewModel
                    {
                        Id = Guid.NewGuid(),
                        HedgeDocumentContentId = x.Id,
                        Order = x.Order,
                        Hidden = content == null ? !x.Required : content.Hidden,
                        HtmlBody = content?.HtmlBody,
                        Name = x.Name,
                        Required = x.Required
                    };
                })];
            }
            else
            {
                // Client 
                CreateTemplateForm.HedgeDocumentContents = [.. templateContentResult.Data
                .Select(x =>
                {
                    var templateDetail = templateResult.DocumentTemplate.HedgeDocumentTemplateDetails
                         .FirstOrDefault(cnt => cnt.HedgeDocumentContentId == x.Id);
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
                })];
                IsCloneFromDPI = true;
            }
            IsActive = templateResult.DocumentTemplate.Enabled;
            CreateTemplateForm.Name = $"CLONE {templateResult.DocumentTemplate.Name}";
            CreateTemplateForm.Description = templateResult.DocumentTemplate.Description;
        }
        else
        {
            CreateDefaultTemplateDetail(currentClientContent);
            if (!IsUpdatedContentEditor)
            {
                _createTemplateDialogComponent.DlgRenameOpen(false);
                IsUpdatedContentEditor = true;
            }
        }

    }

    private void CreateDefaultTemplateDetail(GetDocumentContents.Response result)
    {
        CreateTemplateForm.HedgeDocumentContents = [.. result.Data.Select(x => new HedgeDocumentContentViewModel
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
        var model = (CreateTemplateModel)EditContext.Model;

        IsErrorMessageVisible = !isValid;

        if (model.HedgeDocumentContents.Count == 0)
        {
            await AlertService.ShowToast("There was a system error creating the Hedge Document Template. Please try again.",
                AlertKind.Error, "Failed to create the Hedge Document Template", false);
            return;
        }

        if (IsErrorMessageVisible)
        {
            // Check if validation errors include character limit violations
            var validationMessages = EditContext.GetValidationMessages();
            var hasCharacterLimitError = validationMessages.Any(msg => 
                msg.Contains("maximum character") || 
                msg.Contains("Template name is limited to 50 characters") ||
                msg.Contains("Template description is limited to 150 characters"));

            if (hasCharacterLimitError)
            {
                ErrorMsg = new ErrorMessage
                {
                    Title = "Character limit exceeded",
                    Message = "Template name or description exceeds the maximum character limit. Please shorten the text to proceed."
                };
            }
            else
            {
                ErrorMsg = ContentEmptyValidationMsg;
            }
            return;
        }

        try
        {
            if (IsCloneFromDPI)
            {
                foreach (var content in CreateTemplateForm.HedgeDocumentContents)
                {
                    var requestContent = new CreateDocumentContent.Command(content.Name, ClientId, AllowedBehavior.UserId, content.Required);
                    await MediatorService.Send(requestContent);
                }

                var contentRequest = new GetDocumentContents.Query(ClientId);
                var contentResult = await MediatorService.Send(contentRequest);

                foreach (var content in CreateTemplateForm.HedgeDocumentContents)
                {
                    content.HedgeDocumentContentId = contentResult.Data.FirstOrDefault(c => c.Name == content.Name).Id;
                }
            }

            // New Hedge Document Template
            var request = new CreateDocumentTemplate.Command(ClientId,
            CreateTemplateForm.Name, CreateTemplateForm.Description, CreationMode == DocumentTemplateActions.Duplicate || IsActive, AllowedBehavior.UserId,
            [.. CreateTemplateForm.HedgeDocumentContents.Select(c => c.ToRequestModel())]);

            var result = await MediatorService.Send(request);

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                await AlertService.ShowToast("Hedge Document Template created.", AlertKind.Success, "Success", false);
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
        IsCreateEditContentDialogVisible = false;

        if (result.Success)
        {
            var oldModel = CreateTemplateForm.GetClone();
            IsUpdatedContentEditor = true;
            
            await GetTemplate();

            foreach (var item in CreateTemplateForm.HedgeDocumentContents.ToList())
            {
                var oldcontent = oldModel.HedgeDocumentContents
                    .FirstOrDefault(c => c.HedgeDocumentContentId == item.HedgeDocumentContentId);

                if (oldcontent is not null)
                {
                    CreateTemplateForm.HedgeDocumentContents.Remove(item);
                    CreateTemplateForm.HedgeDocumentContents.Add(oldcontent);
                }
            }

            CreateTemplateForm.Name = oldModel.Name;
            CreateTemplateForm.Description = oldModel.Description;
        }
    }

    private void UpdateTemplateName(CreateTemplateModel createDocumentTemplate)
    {
        CreateTemplateForm = createDocumentTemplate;

        EditContext.NotifyFieldChanged(EditContext.Field(nameof(CreateTemplateForm.Name)));
        EditContext.NotifyFieldChanged(EditContext.Field(nameof(CreateTemplateForm.Description)));
        CreateTemplateNameVisibility = false;
        IsEditMode = true;
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        CreateTemplateForm.Validate(_validationMessageStore);
        EditContext.NotifyValidationStateChanged();
    }

    private void OnToggleClick(HedgeDocumentContentViewModel model)
    {
        var updateObj = CreateTemplateForm.HedgeDocumentContents
                    .FirstOrDefault(x => x.Id == model.Id);

        if (updateObj != null)
        {
            updateObj.Hidden = !model.Hidden;
            CreateTemplateForm.HedgeDocumentContents.Remove(model);
            CreateTemplateForm.HedgeDocumentContents.Add(updateObj);
        }

        StateHasChanged();
    }

    private void OnCancelClick()
    {
        NavigationManager.NavigateTo("hrhedgedocumenttemplate");
    }

    private void DlgRenameOpen()
    {
        _createTemplateDialogComponent.DocumentTemplateActionResult = new DocumentTemplateActionResult(DocumentTemplateActions.Create, false);
        CreateTemplateNameVisibility = true;
        _createTemplateDialogComponent.DlgRenameOpen(true);
    }

    private void CancelCreateEdit()
    {
        IsCreateEditContentDialogVisible = false;
    }

    private async Task EnablePreviewMode()
    {
        var contentRequest = new GetDocumentContents.Query(ClientId);
        var contents = await MediatorService.Send(contentRequest);

        var documentTemplateDetails = CreateTemplateForm.HedgeDocumentContents
            .Where(x => !x.Hidden && !string.IsNullOrWhiteSpace(x.HtmlBody))
            .OrderBy(x => x.Order)
            .Select(c => c.ToRequestModel())
            .ToList();

        HtmlBody = string.Join("", documentTemplateDetails.Select(detail =>
        {
            var associatedHeader = contents.Data
                .FirstOrDefault(x => x.Id == detail.HedgeDocumentContentId);

            return $"<h3 class='dp-preview-content-header'>{(associatedHeader is null ? detail.Name 
                : associatedHeader.Name)}</h3><div class='dp-preview-content-body'>{detail.HtmlBody}<div/>";
        }));

        IsPreviewMode = true;
    }

    private void DisablePreviewMode()
    {
        HtmlBody = string.Empty;

        IsPreviewMode = false;
    }

    public partial class CreateTemplateModel
    {
        public Guid? Id { get; set; } = Guid.Empty;

        [Required(ErrorMessage = "Enter the Template Name.")]
        [MaxLength(50, ErrorMessage = "Template name maximum character is up to 50.")]
        public string Name { get; set; } = "Untitled Document Template";

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

            validationStore.Clear();

            foreach (var item in HedgeDocumentContents ?? [])
            {
                bool isHtmlBodyEmpty = string.IsNullOrEmpty(item.HtmlBody) ||
                    MyRegex().IsMatch(item.HtmlBody);

                bool requiresValidation = item.Required || !item.Hidden;

                if (isHtmlBodyEmpty && requiresValidation)
                {
                    validationStore.Add(
                        new FieldIdentifier(item, nameof(item.HtmlBody)),
                        "Content is required"
                    );
                }
            }
        }

        public CreateTemplateModel GetClone()
        {
            return (CreateTemplateModel)MemberwiseClone();
        }

        [GeneratedRegex(@"^\s*(<p>\s*(<br\s*/?>\s*)*</p>|<br\s*/?>)\s*$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex();
    }
}