namespace DerivativeEDGE.HedgeAccounting.UI.Pages.HedgeRelationship;

public partial class CreateHrDocument
{
    private CreateHrDocumentModel CreateHrDocumentForm { get; set; } = new();

    private CreateHrDocumentDialog _createHrDocumentDialogComponent;

    private FilledHiddenSectionsDialog _filledHiddenSectionsDialog;

    [SupplyParameterFromQuery]
    public long? HedgeRelationshipId { get; set; }

    [Parameter]
    public Guid? TemplateId { get; set; }

    private long ClientId { get; set; }

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

    private bool IsUpdatedContentEditor { get; set; } = false;

    private bool IsActive { get; set; } = true;

    private ErrorMessage ErrorMsg { get; set; }

    private bool IsPreviewMode { get; set; }

    private string HtmlBody { get; set; }

    private bool IsFilledHiddenSectionsDialogVisible { get; set; }

    private static ErrorMessage SystemErrorMsg => new()
    {
        Title = "Failed to create the Hedge Relationship Document",
        Message = "There was a system error creating the Hedge Relationship Document. Please try again."
    };

    private static ErrorMessage ContentEmptyValidationMsg => new()
    {
        Title = "Document content cannot be blank",
        Message = "Fill in the required Hedge Document Content."
    };

    protected override async Task OnInitializedAsync()
    {
        // Initialize Contents
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
            
            await GetHrDocument();

            EditContext = new EditContext(CreateHrDocumentForm);

            _validationMessageStore = new ValidationMessageStore(EditContext);

            EditContext.OnValidationRequested += OnValidationRequested;
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast("There was a problem retrieving document details.", AlertKind.Error,
                "Error", showButton: true);
        }
        finally
        {
            SpinnerService.Hide();
        }

        await base.OnInitializedAsync();
    }

    private async Task GetHrDocument()
    {
        var request = new GetDocumentContents.Query(ClientId);
        var result = await MediatorService.Send(request);

        await GetHrDocumentDetails(result);
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

    private async Task GetHrDocumentDetails(GetDocumentContents.Response currentClientContent)
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
                CreateDefaultHrDocumentDetail(currentClientContent);
                return;

            }

            // Client has a content - Copy only the html body
            if (templates.Count != 0 && currentClientContent.Data.Count != 0)
            {
                var idx = 0;
                CreateHrDocumentForm.HedgeDocumentContents = [.. templates.Select(x =>
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
                CreateHrDocumentForm.HedgeDocumentContents = [.. templateContentResult.Data
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
            }

            CreateHrDocumentForm.Name = $"CLONE {templateResult.DocumentTemplate.Name}";
            CreateHrDocumentForm.Description = templateResult.DocumentTemplate.Description;
        }
        else
        {
            CreateDefaultHrDocumentDetail(currentClientContent);

            if (!IsUpdatedContentEditor)
            {
                _createHrDocumentDialogComponent.DlgRenameOpen(false);
                IsUpdatedContentEditor = true;
            }
        }
    }

    private void CreateDefaultHrDocumentDetail(GetDocumentContents.Response result)
    {
        CreateHrDocumentForm.HedgeDocumentContents = [.. result.Data.Select(x => new HedgeDocumentContentViewModel
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
        var model = (CreateHrDocumentModel)EditContext.Model;

        IsErrorMessageVisible = !isValid;

        if (model.HedgeDocumentContents.Count == 0)
        {
            await AlertService.ShowToast("There was a system error creating the Hedge Relationship Document. Please try again.",
                AlertKind.Error, "Failed to create the Hedge Relationship Document", false);
            return;
        }

        if (IsErrorMessageVisible)
        {
            ErrorMsg = ContentEmptyValidationMsg;
            return;
        }

        var hiddenFilledContents = model.HedgeDocumentContents
            .Where(c => c.Hidden && !string.IsNullOrWhiteSpace(c.HtmlBody))
            .Select(c => c.Name)
            .ToList();

        if (hiddenFilledContents.Count != 0)
        {
            _filledHiddenSectionsDialog.FilledContents = hiddenFilledContents;
            IsFilledHiddenSectionsDialogVisible = true;

            return;
        }

        try
        {
            // New Hedge Relationship Document Content
            var request = new CreateHrDocumentContent.Command(HedgeRelationshipId.GetValueOrDefault(),
            CreateHrDocumentForm.Name, CreateHrDocumentForm.Description, AllowedBehavior.UserId, TemplateId,
            [.. CreateHrDocumentForm.HedgeDocumentContents.Select(c => c.ToRequestModel())]);

            var result = await MediatorService.Send(request);

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                await AlertService.ShowToast("Hedge Document created.", AlertKind.Success, "Success", false);
                await Task.Delay(3000);
                EditContext.MarkAsUnmodified();

                var getHrDocumentRequest = new GetHrDocumentContent.Query(HedgeRelationshipId.GetValueOrDefault(), TemplateId.GetValueOrDefault());
                var getHrDocumentResponse = await MediatorService.Send(getHrDocumentRequest);

                var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId.GetValueOrDefault(), string.Empty, Newtonsoft.Json.JsonConvert.SerializeObject(getHrDocumentResponse), "HAUI", AllowedBehavior.UserId);
                await MediatorService.Send(updateHRCachedData);

                NavigationManager.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId.GetValueOrDefault()}");
            }
        }
        catch (Exception ex)
        {
            IsErrorMessageVisible = true;
            ErrorMsg = SystemErrorMsg;
        }
    }

    private void UpdateDocumentName(CreateHrDocumentModel createHrDocument)
    {
        CreateHrDocumentForm = createHrDocument;
        EditContext.NotifyFieldChanged(EditContext.Field(nameof(CreateHrDocumentForm.Name)));
        EditContext.NotifyFieldChanged(EditContext.Field(nameof(CreateHrDocumentForm.Description)));
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        CreateHrDocumentForm.Validate(_validationMessageStore);
        EditContext.NotifyValidationStateChanged();
    }

    private async void OnCancelClick()
    {
        EditContext.MarkAsUnmodified();

        var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId.GetValueOrDefault(), string.Empty, string.Empty, "HAUI", AllowedBehavior.UserId);
        await MediatorService.Send(updateHRCachedData);

        NavigationManager.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId.GetValueOrDefault()}");
    }

    private void DlgRenameOpen()
    {
        _createHrDocumentDialogComponent.HrDocumentActionResult = new HrDocumentActionResult(HrDocumentActions.Create, false);
        _createHrDocumentDialogComponent.DlgRenameOpen(true);
    }

    private async Task EnablePreviewMode()
    {
        var contentRequest = new GetDocumentContents.Query(ClientId);
        var contents = await MediatorService.Send(contentRequest);

        var hrDocumentDetails = CreateHrDocumentForm.HedgeDocumentContents
            .Where(x => !x.Hidden && !string.IsNullOrWhiteSpace(x.HtmlBody))
            .OrderBy(x => x.Order)
            .Select(c => c.ToRequestModel())
            .ToList();

        var request = new GetHrDocument.Query(HedgeRelationshipId.GetValueOrDefault(),
            TemplateId.GetValueOrDefault());

        var result = await MediatorService.Send(request);

        if (result is not null)
        {
            hrDocumentDetails = [.. result.RelationshipDocumentContents
                    .HedgeRelationshipDocumentContents
                .Select(v => new DocumentTemplateDetail()
                {
                    Hidden = v.Hidden,
                    HedgeDocumentContentId = contents.Data.Count != 0 ?contents.Data
                      .FirstOrDefault(x => x.Name == v.ContentName).Id : Guid.Empty,
                    HtmlBody = v.HtmlBody,
                    Id = Guid.Empty,
                    Name = v.ContentName,
                    Order = v.Order
                })];
        }

        HtmlBody = string.Join("", hrDocumentDetails.Select(detail =>
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

    private void OnToggleClick(HedgeDocumentContentViewModel model)
    {
        var updateObj = CreateHrDocumentForm.HedgeDocumentContents
                    .FirstOrDefault(x => x.Id == model.Id);

        if (updateObj != null)
        {
            updateObj.Hidden = !model.Hidden;
            CreateHrDocumentForm.HedgeDocumentContents.Remove(model);
            CreateHrDocumentForm.HedgeDocumentContents.Add(updateObj);
        }

        StateHasChanged();
    }

    private void CloseFilledHiddenSectionsDialog()
    {
        IsFilledHiddenSectionsDialogVisible = false;
        _filledHiddenSectionsDialog.FilledContents = [];
    }

    public partial class CreateHrDocumentModel
    {
        public Guid? Id { get; set; } = Guid.Empty;

        [Required(ErrorMessage = "Enter the Document Name.")]
        [MaxLength(50, ErrorMessage = "Document name maximum character is up to 50.")]
        public string Name { get; set; } = "Untitled Hedge Relationship Document";

        [Required(ErrorMessage = "Enter the Description.")]
        [MaxLength(150, ErrorMessage = "Document name maximum character is up to 150.")]
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

        public CreateHrDocumentModel GetClone()
        {
            return (CreateHrDocumentModel)MemberwiseClone();
        }

        [GeneratedRegex(@"^\s*(<p>\s*(<br\s*/?>\s*)*</p>|<br\s*/?>)\s*$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex();
    }
}