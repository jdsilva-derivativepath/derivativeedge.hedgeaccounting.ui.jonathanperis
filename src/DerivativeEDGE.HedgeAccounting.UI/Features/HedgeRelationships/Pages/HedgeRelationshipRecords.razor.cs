namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages;

public partial class HedgeRelationshipRecords
{
    #region Parameters
    [Parameter]
    [SupplyParameterFromQuery]
    public long? Id { get; set; }
    #endregion

    #region Public Properties
    public List<Client> AvailableClients { get; set; } = [];
    public bool IsLoadingClients { get; set; }
    public bool IsNewHedgeModalVisible { get; set; }
    public record CancelContext(bool IsUpload, bool IsByClient);
    #endregion

    #region Private Fields
    private bool ShowRegressionModal;
    private bool CancelContextFlag;
    private bool IsRequestByClient;

    private long? SelectedClientId { get; set; }
    private DateTime? CurveDate { get; set; }
    private bool IsDeleteModalVisible { get; set; }
    private long SelectedHedgeRelationshipId { get; set; }

    private bool _clientDropdownInitialized;
    private HedgeRelationshipGrid _gridWrapper = null!;
    private List<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> _hedgeRelationships { get; set; } = [];
    private List<HedgeRelationshipRecordViewModel> _filteredHedgeRelationship { get; set; } = [];
    private readonly List<GridViewModel> _gridViewItems = [];
    private HedgeRelationshipCreate? _newRelationshipModal { get; set; }
    private Dictionary<string, Func<Task>> _actionMap = [];

    private readonly List<string> actionItems =
    [
        "Regression Summary", "Regression Summary All Clients",
        "Upload Regression Summary", "Upload Regression Summary All Clients"
    ];
    private string DeleteMessage => $"Are you sure you want to delete {SelectedHedgeRelationshipId}?";
    private string GetRegressionMessage()
    => CancelContextFlag
        ? "HR Regression Upload request received. Please look for results in the notifications alerts window."
        : "HR Regression Download request received. Please wait and leave this page open for the file to be downloaded on your browser.";

    private bool _isLoadingHedgeRelationships = false;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        CurveDate = DateTime.Now;

        await Task.WhenAll(
            LoadClientsAsync(),
            LoadHedgeRelationshipsAsync()
        );

        _actionMap = new()
        {
            { "Regression Summary", OpenModalDownloadByClient },
            { "Regression Summary All Clients", OpenModalDownloadAllClients },
            { "Upload Regression Summary", OpenModalUploadByClient },
            { "Upload Regression Summary All Clients", OpenModalUploadAllClients }
        };
    }

    #region Action Menu
    private async Task OnActionItemSelected(string selectedItem)
    {
        if (_actionMap.TryGetValue(selectedItem, out var action))
        {
            await action();
        }
    }

    private async Task OpenModalDownloadByClient()
    {
        if (SelectedClientId == null || SelectedClientId == 0)
        {
            await AlertService.ShowToast(
                "Please select a client before performing this operation.",
                AlertKind.Warning,
                "No Client Selected",
                showButton: true
            );
            return;
        }

        ShowRegressionModal = true;
        CancelContextFlag = false; // download
        IsRequestByClient = true;
    }

    private Task OpenModalDownloadAllClients()
    {
        ShowRegressionModal = true;
        CancelContextFlag = false; // download
        IsRequestByClient = false;
        return Task.CompletedTask;
    }

    private async Task OpenModalUploadByClient()
    {
        if (SelectedClientId == null || SelectedClientId == 0)
        {
            await AlertService.ShowToast(
                "Please select a client before performing this operation.",
                AlertKind.Warning,
                "No Client Selected",
                showButton: true
            );
            return;
        }

        ShowRegressionModal = true;
        CancelContextFlag = true; // upload
        IsRequestByClient = true;
    }

    private Task OpenModalUploadAllClients()
    {
        ShowRegressionModal = true;
        CancelContextFlag = true; // upload
        IsRequestByClient = false;
        return Task.CompletedTask;
    }

    #endregion

    #region Modal Regression Summary Handler
    private async Task HandleCancel(CancelContext context)
    {
        var isUpload = context.IsUpload;
        var isByClient = context.IsByClient;

        if (isUpload)
        {
            if (isByClient)
            {
                var result = await Mediator.Send(new UploadRegressionSummaryByClient.Query(SelectedClientId, CurveDate));
                await AlertService.ShowToast(result.Message, result.HasError ? AlertKind.Error : AlertKind.Success, result.HasError ? "Failed" : "Success", showButton: true);
            }
            else
            {
                var result = await Mediator.Send(new UploadRegressionSummaryAllClient.Query(CurveDate));
                await AlertService.ShowToast(result.Message, result.HasError ? AlertKind.Error : AlertKind.Success, result.HasError ? "Failed" : "Success", showButton: true);
            }
        }
        else
        {
            if (isByClient)
            {
                try
                {
                    var response = await Mediator.Send(new DownloadRegressionSummaryByClient.Query(SelectedClientId, CurveDate));
                    using var streamRef = new DotNetStreamReference(stream: response.ExcelStream);
                    await JsRuntime.InvokeVoidAsync("downloadFileFromStream", response.FileName, streamRef);
                }
                catch
                {
                    await AlertService.ShowToast($"Failed to download regression summary: {SelectedClientId}", AlertKind.Error, "Download Error", showButton: true);
                }
            }
            else
            {
                try
                {
                    var response = await Mediator.Send(new DownloadRegressionSummaryAllClient.Query(CurveDate));
                    using var streamRef = new DotNetStreamReference(stream: response.ExcelStream);
                    await JsRuntime.InvokeVoidAsync("downloadFileFromStream", response.FileName, streamRef);
                }
                catch
                {
                    await AlertService.ShowToast("Failed to download regression summary All Client", AlertKind.Error, "Download Error", showButton: true);
                }
            }
        }

        ShowRegressionModal = false;
        CancelContextFlag = false;
    }
    #endregion

    #region Data Loading
    private async Task LoadClientsAsync()
    {
        try
        {
            IsLoadingClients = true;
            var response = await Mediator.Send(new GetClients.Query(), CancellationToken.None);
            response.Clients.Insert(0, new Client { ClientId = 0, ClientName = "All" });
            AvailableClients = response.Clients;
            SelectedClientId = 0; // default to All
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"There was a problem retrieving the Client List: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsLoadingClients = false;
        }
    }

    private async Task LoadHedgeRelationshipsAsync()
    {
        _isLoadingHedgeRelationships = true;
        try
        {
            var response = await Mediator.Send(new GetHedgeRelationship.Query());
            _hedgeRelationships = response.HedgeRelationships ?? [];
            FilterHedgeRelationshipsByClient();
        }
        finally
        {
            _isLoadingHedgeRelationships = false;
        }
    }
    #endregion

    #region Client Sticky + Filtering
    public async Task ClientComboboxCreated()
    {
        await SetStickyClient();
        _clientDropdownInitialized = true;
    }

    private async Task SetStickyClient()
    {
        var selectedClientId = await LocalStorage.GetItemAsync<long?>(StringConstants.HedgeRelationshipSelectedClientId);
        if (selectedClientId > 0)
        {
            SelectedClientId = selectedClientId;
            FilterHedgeRelationshipsByClient();
        }
    }

    private async Task OnClientChanged(long? newClientId)
    {
        SelectedClientId = newClientId;
        await LocalStorage.SetItemAsync(StringConstants.HedgeRelationshipSelectedClientId, SelectedClientId);
        FilterHedgeRelationshipsByClient();
    }

    private Task OnCurveDateChanged(DateTime? date)
    {
        CurveDate = date;
        return Task.CompletedTask;
    }

    private void FilterHedgeRelationshipsByClient()
    {
        var data = _hedgeRelationships ?? [];
        var filteredData = (SelectedClientId == null || SelectedClientId == 0)
            ? data
            : data.Where(u => u.ClientID == SelectedClientId);

        _filteredHedgeRelationship = [.. filteredData.Select(x => new HedgeRelationshipRecordViewModel(x))];
    }
    #endregion

    #region New Relationship
    private async Task HandleNewRelationshipClick()
    {
        if (_newRelationshipModal == null) return;
        IsNewHedgeModalVisible = true;
        _newRelationshipModal.HedgeRelationship.ID = 0;
        _newRelationshipModal.HedgeRelationship.ClientID = SelectedClientId.GetValueOrDefault();
        await _newRelationshipModal.LoadClientEntitiesAsync(SelectedClientId);
    }
    #endregion

    #region Delete Flow
    private Task HandleDeleteItemClick(long hedgeRelationshipId)
    {
        SelectedHedgeRelationshipId = hedgeRelationshipId;
        IsDeleteModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseDeleteModal()
    {
        IsDeleteModalVisible = false;
        return Task.CompletedTask;
    }

    private async Task ConfirmDelete()
    {
        IsDeleteModalVisible = false;
        var result = await Mediator.Send(new DeleteHedgeRelationship.Command(SelectedHedgeRelationshipId));
        await AlertService.ShowToast(result.Message, result.HasError ? AlertKind.Error : AlertKind.Success, result.HasError ? "Failed" : "Success", showButton: true);
        await LoadHedgeRelationshipsAsync();
    }
    #endregion

    #region Grid View CRUD (Save/SaveAs/Delete)
    private async Task ViewCrudHandler(GridViewModel view)
    {
        // Map string to enum where possible (non-breaking)
        GridCrudRequestType? requestType = view.RequestType switch
        {
            "Save" => GridCrudRequestType.Save,
            "SaveAs" => GridCrudRequestType.SaveAs,
            "Delete" => GridCrudRequestType.Delete,
            _ => null
        };

        switch (requestType)
        {
            case GridCrudRequestType.Save:
                await HandleSave(view);
                break;
            case GridCrudRequestType.SaveAs:
                await HandleSaveAs(view);
                break;
            case GridCrudRequestType.Delete:
                HandleDelete(view);
                break;
            default:
                // Unknown request type from component library – ignore safely
                break;
        }
    }

    private async Task HandleSave(GridViewModel view)
    {
        var existing = _gridViewItems.FirstOrDefault(g => g.Filter == view.Filter);
        if (existing != null)
        {
            existing.GridSettingsJson = view.GridSettingsJson;
        }
        // Let the grid switch to the saved view
        await Task.Delay(100);
        _gridWrapper?.SetView(view.Filter);
    }

    private async Task HandleSaveAs(GridViewModel view)
    {
        var gridview = new GridViewModel
        {
            Filter = view.Filter,
            GridSettingsJson = view.GridSettingsJson,
            Id = view.Id,
            IsDefault = view.IsDefault
        };
        _gridViewItems.Add(gridview);
        await Task.Delay(100);
        _gridWrapper?.SetView(view.Filter);
    }

    private void HandleDelete(GridViewModel view)
    {
        var toRemove = _gridViewItems.FirstOrDefault(g => g.Filter == view.Filter);
        if (toRemove != null)
        {
            _gridViewItems.Remove(toRemove);
        }
    }
    #endregion

    #region Row Click Navigation
    private void OnRowClickedHandler(HedgeRelationshipRecordViewModel selectedRecord)
    {
        if (selectedRecord is null) return;
        NavManager.NavigateTo($"{NavManager.BaseUri}hedgerelationship?Id={selectedRecord.ID}");
    }
    #endregion
}