namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentContent;

public partial class Contents
{
    private long? _prevSelectedClientId;
    private ContentDialog _contentDialogComponent;
    private UsedContentDialog _usedContentDialogComponent;
    private DeleteDialog _deleteContentDialogComponent;

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    public IAlertService AlertService { get; set; } = default!;

    [Inject]
    public SpinnerService SpinnerService { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Parameter]
    public long? SelectedClientId { get; set; }

    [Parameter]
    public EventCallback<int> OnContentsListUpdated { get; set; }

    [Parameter]
    public EventCallback<ErrorMessage> OnActionFailed { get; set; }

    [Parameter]
    public EventCallback OnActionTriggerred { get; set; }

    [Parameter]
    public AllowedBehavior AllowedBehavior { get; set; }

    private bool IsVisibleGridCover => ContentsList != null && ContentsList.Count == 0;

    public List<Models.DocumentContent> ContentsList { get; set; }

    private DefaultGrid<Models.DocumentContent> ContentsGrid { get; set; }

    public bool IsCreateEditContentDialogVisible { get; set; }

    private bool IsUsedContentDialogVisible { get; set; }

    private bool IsDeleteDialogVisible { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (_prevSelectedClientId != SelectedClientId)
        {
            _prevSelectedClientId = SelectedClientId;
            await LoadContents(SelectedClientId.GetValueOrDefault());
        }
        await base.OnParametersSetAsync();
    }

    public async Task LoadContents(long clientId)
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
                await UpdateContentsList([]);
                return;
            }

            await LoadContents();
        }
        catch (Exception)
        {
            await AlertService.ShowToast("There was a problem retrieving the client Hedge Document Contents.", 
                AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            SpinnerService.Hide();
        }
    }

    private async Task LoadContents()
    {
        var response = await Mediator.Send(new GetDocumentContents.Query(SelectedClientId.GetValueOrDefault()));
        await UpdateContentsList(response.Data);
    }

    private async Task UpdateContentsList(List<Models.DocumentContent> documentContents)
    {
        ContentsList = documentContents;
        await OnContentsListUpdated.InvokeAsync(ContentsList.Count);
    }

    private async Task OnActionSelected(MenuEventArgs args, Models.DocumentContent data)
    {
        await OnActionTriggerred.InvokeAsync();
        if (args.Item.Text == DocumentContentActions.Edit)
        {
            IsCreateEditContentDialogVisible = true;
            _contentDialogComponent.Model = new Models.DocumentContent {
                Id = data.Id,
                Name = data.Name,
                Required = data.Required,
                Order = data.Order
            };
        }
        else if (args.Item.Text == DocumentContentActions.Delete)
        {
            if (await CheckIfUsed(data) == false)
            {
                _deleteContentDialogComponent.ToDelete = data;
                IsDeleteDialogVisible = true;
            }
        }
    }

    private void CancelCreateEdit()
    {
        IsCreateEditContentDialogVisible = false;
    }

    private void CancelDelete()
    {
        IsDeleteDialogVisible = false;
    }

    private async Task UpdatePage(DocumentContentActionResult result)
    {
        IsCreateEditContentDialogVisible = false;
        IsDeleteDialogVisible = false;
        if (result.Success)
        {
            await LoadContents(SelectedClientId.GetValueOrDefault());
            return;
        }
        await OnActionFailed.InvokeAsync(result.ErrorMessage);
    }

    private void CloseUsedContentDialog()
    {
        IsUsedContentDialogVisible = false;
        _usedContentDialogComponent.TemplateNames = [];
    }

    private void ViewTemplates()
    {
        IsUsedContentDialogVisible = false;
        _usedContentDialogComponent.TemplateNames = [];
        NavigationManager.NavigateTo("hrhedgedocumenttemplate");
    }

    private async Task<bool?> CheckIfUsed(Models.DocumentContent content)
    {
        try
        {
            SpinnerService.Show();
            var response = await Mediator.Send(new GetDocumentTemplateNames.Query(SelectedClientId.GetValueOrDefault(), content.Id));
            if (response.TemplateNames?.Count > 0)
            {
                _usedContentDialogComponent.ContentName = content.Name;
                _usedContentDialogComponent.TemplateNames = response.TemplateNames;
                IsUsedContentDialogVisible = true;
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            await OnActionFailed.InvokeAsync(new ErrorMessage
            {
                Title = "Failed to delete the Hedge Document Content",
                Message = "There was a system error deleting the Hedge Document Content. Please try again."
            });
            return null;
        }
        finally
        {
            SpinnerService.Hide();
        }
    }
}
