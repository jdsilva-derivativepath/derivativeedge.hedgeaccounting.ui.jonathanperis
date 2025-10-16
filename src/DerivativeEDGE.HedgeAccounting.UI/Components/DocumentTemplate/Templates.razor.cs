namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentTemplate;

public partial class Templates
{
    private long? _prevSelectedClientId;
    private DeleteTemplateDialog _deleteTemplateDialogComponent;
    private PreviewTemplateDialog _previewTemplateDialogComponent;

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    public IAlertService AlertService { get; set; } = default!;

    [Inject]
    public SpinnerService SpinnerService { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    private ILocalStorageService LocalStorage { get; set; }

    [Parameter]
    public long? SelectedClientId { get; set; }

    [Parameter]
    public EventCallback<ErrorMessage> OnActionFailed { get; set; }

    [Parameter]
    public EventCallback OnActionTriggerred { get; set; }

    [Parameter]
    public AllowedBehavior AllowedBehavior { get; set; }

    private static ErrorMessage SetStateErrorMessage (bool enable) => new()
    {
        Title = $"Failed to {(enable ? "activate" : "deactivate")} the Hedge Document Template",
        Message = $"There was a system error {(enable ? "activating" : "deactivating")} the Hedge Document Template. " +
                $"Please try again."
    };

    private bool IsVisibleGridCover => TemplatesList != null && TemplatesList.Count == 0;

    private List<Models.DocumentTemplate> TemplatesList { get; set; }

    private List<Models.DocumentContent> ContentsList { get; set; }

    private DefaultGrid<Models.DocumentTemplate> TemplatesGrid { get; set; }

    private bool IsDeleteDialogVisible { get; set; }

    private bool IsPreviewDialogVisible { get; set; }

    private bool listViewSelected = true; //Default view if page never have been used before.
    private bool galleryViewSelected;
    private bool matrixViewSelected;

    private string SearchString = string.Empty;

    private string ActiveView { get; set; } = "list"; //Default view if page never have been used before.

    private SfTextBox? SearchBox { get; set; }

    private List<CardItem<Models.DocumentTemplate>> CardItemsList { get; set; } = [];

    // filter card set based on search toolbar
    private IEnumerable<CardItem<Models.DocumentTemplate>> FilteredItems => CardItemsList.Where(d => string.IsNullOrEmpty(SearchString) || d.HeaderText.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnParametersSetAsync()
    {
        if (_prevSelectedClientId != SelectedClientId)
        {
            _prevSelectedClientId = SelectedClientId;
            await LoadTemplates(SelectedClientId.GetValueOrDefault());
        }
        await base.OnParametersSetAsync();
    }

    private async Task LoadTemplates(long clientId)
    {
        try
        {
            if (SelectedClientId != clientId)
            {
                SelectedClientId = clientId;
            }

            SpinnerService.Show();

            if (SelectedClientId <= 0)
            {
                await UpdateTemplatesList([], []);
                return;
            }

            await LoadTemplates();
        }
        catch (Exception)
        {
            await AlertService.ShowToast("There was a problem retrieving the client Hedge Document Templates.",
                AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            SpinnerService.Hide();
        }
    }

    private async Task LoadTemplates()
    {
        var contents = await Mediator.Send(new GetDocumentContents.Query(SelectedClientId.GetValueOrDefault()));
        var templates = await Mediator.Send(new ListDocumentTemplates.Query(SelectedClientId.GetValueOrDefault()));
        await UpdateTemplatesList(contents.Data, templates.DocumentTemplates);
    }

    private async Task UpdateTemplatesList(List<Models.DocumentContent> documentContents, List<Models.DocumentTemplate> documentTemplates)
    {
        ContentsList = [.. documentContents.OrderBy(x => x.Order)];
        TemplatesList = [.. documentTemplates.OrderBy(x => x.Name)];
        CardItemsList = [.. documentTemplates.OrderBy(x => x.Name).Select(x => new CardItem<Models.DocumentTemplate>()
        {
            CardIconCss = "fa-regular fa-file",
            HeaderText = x.Name,
            Description = x.Description,
            FirstSubText = $"Updated by: {x.ModifiedBy}",
            SecondarySubText = $"Last Updated: {x.ModifiedOn.GetValueOrDefault():yyyy/MM/dd HH:mm:ss}",
            ActionMenuItems = ["Edit", "Preview", "Duplicate", (x.Enabled ? "Deactivate" : "Activate"), "Delete"],
            DataSource = x,
        })];
        await GetActiveView();
    }

    private async Task OnActionSelected(MenuEventArgs args, Models.DocumentTemplate data)
    {
        await OnActionTriggerred.InvokeAsync();
        if (args.Item.Text == DocumentTemplateActions.Edit)
        {
            await SetCreationMode(DocumentTemplateActions.Edit);
            NavigationManager.NavigateTo($"edit-template/{data.Id}");
        }
        else if (args.Item.Text == DocumentTemplateActions.Preview)
        {
            await _previewTemplateDialogComponent.FormatContent(data.ClientId, data.Name, data.HedgeDocumentTemplateDetails);
            IsPreviewDialogVisible = true;
        }
        else if (args.Item.Text == DocumentTemplateActions.Duplicate)
        {
            await SetCreationMode(DocumentTemplateActions.Duplicate);
            NavigationManager.NavigateTo($"create-template/{data.Id}");
        }
        else if (args.Item.Text == DocumentTemplateActions.Activate)
        {
            await SetTemplateState(true, data.Id);
        }
        else if (args.Item.Text == DocumentTemplateActions.Deactivate)
        {
            await SetTemplateState(false, data.Id);
        }
        else if (args.Item.Text == DocumentTemplateActions.Delete)
        {
            _deleteTemplateDialogComponent.ToDelete = data;
            IsDeleteDialogVisible = true;
        }
    }

    private void CancelDelete()
    {
        IsDeleteDialogVisible = false;
    }

    private void ClosePreview()
    {
        IsPreviewDialogVisible = false;
    }

    private async Task UpdatePage(DocumentTemplateActionResult result)
    {
        IsDeleteDialogVisible = false;
        if (result.Success)
        {
            await LoadTemplates(SelectedClientId.GetValueOrDefault());
            return;
        }
        await OnActionFailed.InvokeAsync(result.ErrorMessage);
    }

    private void DoSearchHandler()
    {
        switch (ActiveView)
        {
            case "list":
                TemplatesGrid?.InnerGrid.SearchAsync(SearchString);
                break;
            case "gallery":
                break;
            case "matrix":
                TemplatesGrid?.InnerGrid.SearchAsync(SearchString);
                break;
        }
    }

    private async Task ChangeListView(string view)
    {
        SearchString = "";
        ActiveView = view;
        await SetActiveView();
    }

    private void OnCardButtonClickHandler(CardItem card)
    {
        // code here for action button
    }

    private async Task OnCardIconClickHandler((string, CardItem) args)
    {
        var selectedCardItem = args.Item2 as CardItem<Models.DocumentTemplate>;
        var template = selectedCardItem.DataSource;

        if (args.Item1 == DocumentTemplateActions.Edit)
        {
            await SetCreationMode(DocumentTemplateActions.Edit);
            NavigationManager.NavigateTo($"edit-template/{template.Id}");
        }
        else if (args.Item1 == DocumentTemplateActions.Preview)
        {
            await _previewTemplateDialogComponent.FormatContent(template.ClientId, template.Name, template.HedgeDocumentTemplateDetails);
            IsPreviewDialogVisible = true;
        }
        else if (args.Item1 == DocumentTemplateActions.Duplicate)
        {
            await SetCreationMode(DocumentTemplateActions.Duplicate);
            NavigationManager.NavigateTo($"create-template/{template.Id}");
        }
        else if (args.Item1 == DocumentTemplateActions.Activate)
        {
            await SetTemplateState(true, template.Id);
        }
        else if (args.Item1 == DocumentTemplateActions.Deactivate)
        {
            await SetTemplateState(false, template.Id);
        }
        else if (args.Item1 == DocumentTemplateActions.Delete)
        {
            _deleteTemplateDialogComponent.ToDelete = template;
            IsDeleteDialogVisible = true;
        }
    }

    private async Task GetActiveView()
    {
        ActiveView = await LocalStorage!.GetItemAsync<string>(StringConstants.HedgeDocumentTemplateActiveView);
        ChangeActiveButtonView(ActiveView);
    }

    private async Task SetActiveView()
    {
        await LocalStorage!.SetItemAsync(StringConstants.HedgeDocumentTemplateActiveView, ActiveView);
        ChangeActiveButtonView(ActiveView);
    }

    private async Task SetCreationMode(string actions)
    {
        await LocalStorage!.SetItemAsync(StringConstants.HedgeDocumentTemplateMode, actions);
    }

    private void ChangeActiveButtonView(string view)
    {
        switch (view)
        {
            case "list":
                listViewSelected = true;
                galleryViewSelected = false;
                matrixViewSelected = false;
                break;
            case "gallery":
                galleryViewSelected = true;
                listViewSelected = false;
                matrixViewSelected = false;
                break;
            case "matrix":
                matrixViewSelected = true;
                listViewSelected = false;
                galleryViewSelected = false;
                break;
            default:
                listViewSelected = true;
                galleryViewSelected = false;
                matrixViewSelected = false;
                ActiveView = "list";
                break;
        }
    }

    private async Task SetTemplateState(bool enable, Guid id)
    {
        try
        {
            await OnActionTriggerred.InvokeAsync();
            SpinnerService.Show();

            // Retrieve the latest record from DB
            var response = await Mediator.Send(new GetDocumentTemplate.Query(id));

            // Update state
            var updateResponse = await Mediator.Send(new UpdateDocumentTemplate.Command(response.DocumentTemplate.Id, 
                response.DocumentTemplate.Name, 
                response.DocumentTemplate.Description, 
                enable, AllowedBehavior.UserId, 
                response.DocumentTemplate.HedgeDocumentTemplateDetails));

            var success = !string.IsNullOrEmpty(updateResponse.Message);
            if (success)
            {
                await RefreshGridRowData(id);

                await AlertService.ShowToast($"Hedge Document Template {(enable ? "activated" : "deactivated")}.",
                        AlertKind.SuccessWithoutContent, true);
                return;
            }
            await OnActionFailed.InvokeAsync(SetStateErrorMessage(enable));
        }
        catch (Exception)
        {
            await OnActionFailed.InvokeAsync(SetStateErrorMessage(enable));
        }
        finally
        {
            SpinnerService.Hide();
        }
    }

    private async Task RefreshGridRowData(Guid id)
    {
        var getUpdatedTemplateResponse = await Mediator.Send(new GetDocumentTemplate.Query(id));
        var updatedTemplate = getUpdatedTemplateResponse.DocumentTemplate;
        var gridRowData = TemplatesList.Where(t => t.Id == id).FirstOrDefault();
        gridRowData.Enabled = updatedTemplate.Enabled;
        gridRowData.ModifiedOn = updatedTemplate.ModifiedOn;
        gridRowData.ModifiedById = updatedTemplate.ModifiedById;
        gridRowData.ModifiedBy = updatedTemplate.ModifiedBy;
        await TemplatesGrid.InnerGrid.Refresh();

        var cardRowData = CardItemsList.Where(c => c.DataSource.Id == id).FirstOrDefault();
        cardRowData.FirstSubText = $"Updated by: {updatedTemplate.ModifiedBy}";
        cardRowData.SecondarySubText = $"Last Updated: {updatedTemplate.ModifiedOn.GetValueOrDefault():yyyy/MM/dd HH:mm:ss}";
        cardRowData.ActionMenuItems = ["Edit", "Duplicate", (updatedTemplate.Enabled ? "Deactivate" : "Activate"), "Delete"];
        cardRowData.DataSource.Enabled = updatedTemplate.Enabled;
        cardRowData.DataSource.ModifiedOn = updatedTemplate.ModifiedOn;
        cardRowData.DataSource.ModifiedById = updatedTemplate.ModifiedById;
        cardRowData.DataSource.ModifiedBy = updatedTemplate.ModifiedBy;
    }

    private async Task OnMatrixItemSelected(Models.DocumentTemplate documentTemplate, Guid contentId)
    {
        var documentTemplateDetails = documentTemplate.HedgeDocumentTemplateDetails.Where(x => x.HedgeDocumentContentId == contentId).ToList();

        await _previewTemplateDialogComponent.FormatContent(documentTemplate.ClientId, documentTemplate.Name, documentTemplateDetails);
        IsPreviewDialogVisible = true;
    }
}
