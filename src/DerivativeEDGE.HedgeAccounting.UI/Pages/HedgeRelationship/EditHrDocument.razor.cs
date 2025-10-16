namespace DerivativeEDGE.HedgeAccounting.UI.Pages.HedgeRelationship;

public partial class EditHrDocument
{
    private const string changeHrDocument = "Change Document";
    private readonly List<string> actionItems = [changeHrDocument];

    private EditHrDocumentModel EditHrDocumentForm { get; set; } = new();

    private EditHrDocumentDialog _editHrContentDialogComponent;
    private ConfirmationContentDialog _confirmationContentDialogComponent;

    private FilledHiddenSectionsDialog _filledHiddenSectionsDialog;

    [Parameter]
    public Guid? TemplateId { get; set; }

    [SupplyParameterFromQuery]
    public long ClientId { get; set; }

    [SupplyParameterFromQuery]
    public long HedgeRelationshipId { get; set; }

    [SupplyParameterFromQuery]
    public bool? IsChangingTemplate { get; set; }

    EditContext EditContext;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IAlertService AlertService { get; set; } = default!;

    [Inject]
    private IMediator MediatorService { get; set; } = default!;

    [Inject]
    public SpinnerService SpinnerService { get; set; } = default!;

    private AllowedBehavior AllowedBehavior { get; set; } = default!;

    private ValidationMessageStore _validationMessageStore;

    private bool IsErrorMessageVisible { get; set; }

    private bool IsCloneFromDPI { get; set; }

    private bool IsUpdatedContentEditor { get; set; } = false;

    private bool IsActive { get; set; } = true;

    private ErrorMessage ErrorMsg { get; set; }

    private bool IsPreviewMode { get; set; } = false;

    private string HtmlBody { get; set; }

    private int ConfirmedSmartagsCount { get; set; }

    private int IncompleteSmartTagsCount { get; set; }

    private bool IsFilledHiddenSectionsDialogVisible { get; set; }

    private static ErrorMessage SystemErrorMsg => new()
    {
        Title = "Failed to edit the Hedge Relationship Document",
        Message = "There was a system error editing the Hedge Relationship Document. Please try again."
    };

    private static ErrorMessage ContentEmptyValidationMsg => new()
    {
        Title = "Document content cannot be blank",
        Message = "Fill in the required Hedge Document Content."
    };

    protected override async Task OnInitializedAsync()
    {
        SpinnerService.Show();

        await CheckAccessRights();

        if (!IsChangingTemplate.GetValueOrDefault())
        {
            await GetCurrentDocument();
        }
        else
        {
            await GetNewHrDocument();
        }

        EditContext = new EditContext(EditHrDocumentForm);

        _validationMessageStore = new ValidationMessageStore(EditContext);
        // wire up an event handler to trigger validation events emitted by the Edit Context

        EditContext.OnValidationRequested += OnValidationRequested;
        SpinnerService.Hide();

        await base.OnInitializedAsync();
    }

    private async Task CheckAccessRights()
    {
        try
        {
            SpinnerService.Show();

            var query = new GetAllowedBehavior.Query();
            var response = await MediatorService.Send(query, CancellationToken.None);

            AllowedBehavior = response.AllowedBehavior;
        }
        catch (Exception)
        {
            await AlertService.ShowToast("There was a problem retrieving the user's access rights.", AlertKind.Error,
                "Error", showButton: true);
        }
        finally
        {
            SpinnerService.Hide();
        }
    }

    private async Task GetCurrentDocument()
    {
        var request = new GetHrDocument.Query(HedgeRelationshipId, Guid.Empty, false);
        var result = await MediatorService.Send(request);

        if (result is not null && result.RelationshipDocumentContents is not null || HedgeRelationshipId is not 0)
        {
            if (result?.RelationshipDocumentContents?.HRDeDesignated ?? false)
            {
                // Edit Document is disabled when the hedge relationship is de-designated. Redirect to the hedge relationship page.
                var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId, string.Empty, string.Empty, "HAUI", AllowedBehavior.UserId);
                await MediatorService.Send(updateHRCachedData);
                NavigationManager.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId}");
                return;
            }

            EditHrDocumentForm.HedgeDocumentContents = [.. result.RelationshipDocumentContents.HedgeRelationshipDocumentContents
            .Select(x =>
            {
                return new HedgeDocumentContentViewModel()
                {
                    Id = Guid.NewGuid(),
                    //HedgeDocumentContentId = x.Id,
                    Order = x.Order,
                    Hidden = x.Hidden,
                    HtmlBody = x.HtmlBody,
                    Name = x.ContentName,
                    //Required = x.Required
                };

            })];

            TemplateId = result.RelationshipDocumentContents.HedgeDocumentTemplateId;
            EditHrDocumentForm.Id = result.RelationshipDocumentContents.Id;
            EditHrDocumentForm.Name = result.RelationshipDocumentContents.DocumentName;
            EditHrDocumentForm.Description = result.RelationshipDocumentContents.DocumentDescription;
        }
    }

    private async Task GetNewHrDocument()
    {
        var request = new GetDocumentContents.Query(ClientId);
        var result = await MediatorService.Send(request);
        await GetHrDocumentDetails(result);
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
                CreateNewDefaultHrDocumentDetail(currentClientContent);
                return;

            }
            // Client has a content - Copy only the html body
            if (templates.Count != 0 && currentClientContent.Data.Count != 0)
            {
                var idx = 0;
                EditHrDocumentForm.HedgeDocumentContents = [.. templates.Select(x =>
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
                EditHrDocumentForm.HedgeDocumentContents = [.. templateContentResult.Data
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

            IsActive = templateResult.DocumentTemplate.Enabled;
            EditHrDocumentForm.Name = $"CLONE {templateResult.DocumentTemplate.Name}";
            EditHrDocumentForm.Description = templateResult.DocumentTemplate.Description;
        }
        else
        {
            CreateNewDefaultHrDocumentDetail(currentClientContent);
            if (!IsUpdatedContentEditor)
            {
                _editHrContentDialogComponent.DlgRenameOpen(false);
                IsUpdatedContentEditor = true;
            }
        }
    }

    private void CreateNewDefaultHrDocumentDetail(GetDocumentContents.Response result)
    {
        EditHrDocumentForm.HedgeDocumentContents = [.. result.Data.Select(x => new HedgeDocumentContentViewModel
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
        var model = (EditHrDocumentModel)EditContext.Model;

        IsErrorMessageVisible = !isValid;

        if (model.HedgeDocumentContents.Count == 0)
        {
            await AlertService.ShowToast("There was a system error creating the Hedge Relationship Document. Please try again.",
                AlertKind.Error, "Failed to edit the Hedge Relationship Document", false);
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
            // Edit Save
            var request = new UpdateHrDocumentContent.Command(HedgeRelationshipId, EditHrDocumentForm.Name, EditHrDocumentForm.Description, 
                AllowedBehavior.UserId, TemplateId, IsChangingTemplate.GetValueOrDefault(),
                [.. EditHrDocumentForm.HedgeDocumentContents.Select(c => c.ToRequestModel())]);

            var result = await MediatorService.Send(request);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                await CountSmartTagsUsed();

                _confirmationContentDialogComponent.DlgButtonOpen();
            }
        }
        catch (Exception ex)
        {
            IsErrorMessageVisible = true;
            ErrorMsg = SystemErrorMsg;
        }
    }

    private void UpdateHrDocumentName(EditHrDocumentModel createHrDocument)
    {
        EditHrDocumentForm = createHrDocument;
    }

    private void OnValidationRequested(object sender, ValidationRequestedEventArgs e)
    {
        EditHrDocumentForm.Validate(_validationMessageStore);
        EditContext.NotifyValidationStateChanged();
    }

    private async Task OnCancelClick()
    {
        var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId, string.Empty, string.Empty, "HAUI", AllowedBehavior.UserId);
        await MediatorService.Send(updateHRCachedData);

        NavigationManager.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId}");
    }

    private void DlgRenameOpen()
    {
        _editHrContentDialogComponent.HrDocumentActionResult = new HrDocumentActionResult(HrDocumentActions.Edit, false);
        _editHrContentDialogComponent.DlgRenameOpen(true);
    }

    private async Task EnablePreviewMode()
    {
        var contentRequest = new GetDocumentContents.Query(ClientId);
        var contents = await MediatorService.Send(contentRequest);

        var hrDocumentDetails = EditHrDocumentForm.HedgeDocumentContents
            .Where(x => !x.Hidden && !string.IsNullOrWhiteSpace(x.HtmlBody))
            .OrderBy(x => x.Order)
            .Select(c => c.ToRequestModel())
            .ToList();

        var keywordRequest = new GetHrDocumentKeywordValue.Query(HedgeRelationshipId);
        var keywords = await MediatorService.Send(keywordRequest);

        if (keywords is not null &&
            keywords.DocumentKeywordValues is not null &&
            keywords.DocumentKeywordValues.KeywordValues is not null &&
            keywords.DocumentKeywordValues.KeywordValues.Count != 0)
        {
            hrDocumentDetails =
                ReplaceKeywords([.. hrDocumentDetails], keywords.DocumentKeywordValues.KeywordValues);
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

    private void OnActionItemSelected(string selectedItem)
    {
        if (selectedItem == changeHrDocument)
        {
            IsChangingTemplate = true;
            EditContext.MarkAsUnmodified();
            NavigationManager.NavigateTo($"gallery-hrdocument?ClientId={ClientId}&HedgeRelationshipId={HedgeRelationshipId}&IsChangingTemplate={IsChangingTemplate}");
        }
    }

    private async Task CountSmartTagsUsed()
    {
        var keywordRequest = new GetHrDocumentKeywordValue.Query(HedgeRelationshipId);
        var keywords = await MediatorService.Send(keywordRequest);

        if (keywords?.DocumentKeywordValues?.KeywordValues is not { Count: > 0 })
        {
            ConfirmedSmartagsCount = 0;
            IncompleteSmartTagsCount = 0;
            return;
        }

        int confirmed = 0;
        int incomplete = 0;
        var keyvaluePairs = keywords.DocumentKeywordValues.KeywordValues;

        foreach (var pair in keyvaluePairs)
        {
            string tagPattern = $"{{{{{pair.Key}}}}}";

            bool tagFound = EditHrDocumentForm.HedgeDocumentContents
                .Where(c => !string.IsNullOrWhiteSpace(c.HtmlBody))
                .Any(c => c.HtmlBody.Contains(tagPattern, StringComparison.OrdinalIgnoreCase));

            if (tagFound)
            {
                if (!string.IsNullOrWhiteSpace(pair.Value))
                {
                    confirmed++;
                }
                else
                {
                    incomplete++;
                }
            }
        }

        ConfirmedSmartagsCount = confirmed;
        IncompleteSmartTagsCount = incomplete;
    }

    private static List<DocumentTemplateDetail>
        ReplaceKeywords(List<DocumentTemplateDetail> contents,
        Dictionary<string, string> keyvaluePairs)
    {
        var updatedContents = new List<DocumentTemplateDetail>();
        var regexPatterns = keyvaluePairs.Where(c => !string.IsNullOrWhiteSpace(c.Value)).Select(keyword =>
        {
            return new Regex($"{{{{{keyword.Key}}}}}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }).ToList();

        foreach (var item in contents.Where(c => !string.IsNullOrWhiteSpace(c.HtmlBody)))
        {
            int i = 0;
            foreach (var kvp in keyvaluePairs.Where(c => !string.IsNullOrWhiteSpace(c.Value)))
            {
                item.HtmlBody = regexPatterns[i].Replace(item.HtmlBody, kvp.Value);
                i++;
            }
            updatedContents.Add(item);
        }

        return [.. updatedContents.OrderBy(c => c.Order)];
    }

    private void OnToggleClick(HedgeDocumentContentViewModel model)
    {
        var updateObj = EditHrDocumentForm.HedgeDocumentContents
                    .FirstOrDefault(x => x.Id == model.Id);

        if (updateObj != null)
        {
            updateObj.Hidden = !model.Hidden;
            EditHrDocumentForm.HedgeDocumentContents.Remove(model);
            EditHrDocumentForm.HedgeDocumentContents.Add(updateObj);
        }

        StateHasChanged();
    }

    private void CloseFilledHiddenSectionsDialog()
    {
        IsFilledHiddenSectionsDialogVisible = false;
        _filledHiddenSectionsDialog.FilledContents = [];
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

    public class EditHrDocumentModel
    {
        public Guid Id { get; set; }

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

        public EditHrDocumentModel GetClone()
        {
            return (EditHrDocumentModel)MemberwiseClone();
        }
    }
}