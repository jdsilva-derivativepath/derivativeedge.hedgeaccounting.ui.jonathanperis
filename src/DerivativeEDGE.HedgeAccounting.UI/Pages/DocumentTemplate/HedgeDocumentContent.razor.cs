namespace DerivativeEDGE.HedgeAccounting.UI.Pages.DocumentTemplate;

public partial class HedgeDocumentContent
{
    private bool _clientDropdownInitialized;
    private int _contentsListCount;
    private Contents _contentsComponent;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; }

    [Inject]
    private IAlertService AlertService { get; set; }

    [Inject]
    private IMediator Mediator { get; set; }

    [Inject]
    private SpinnerService SpinnerService { get; set; }

    public AllowedBehavior AllowedBehavior { get; set; }

    public long? SelectedClientId { get; set; }

    public List<ClientName> Clients { get; private set; } = [];

    public bool IsCreateContentButtonVisible => _contentsListCount < 8 && SelectedClientId > 0;
    
    public bool IsErrorMessageVisible { get; set; }

    private string ErrorTitle { get; set; }

    private string ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await CheckAccessRights();
        await LoadClients();

        if (!AllowedBehavior.IsDpiUser)
        {
            SelectedClientId = AllowedBehavior.ClientId;
        }

        await base.OnInitializedAsync();
    }

    public async Task CheckAccessRights()
    {
        try
        {
            SpinnerService.Show();

            var query = new GetAllowedBehavior.Query();
            var response = await Mediator.Send(query, CancellationToken.None);

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

    public async Task LoadClients()
    {
        try
        {
            if (!AllowedBehavior.IsDpiUser)
            {
                return;
            }

            SpinnerService.Show();

            var query = new GetAllClientNames.Query();
            var response = await Mediator.Send(query, CancellationToken.None);

            Clients = response.ClientNames;
        }
        catch (Exception)
        {
            await AlertService.ShowToast("There was a problem retrieving the Client List.", AlertKind.Error, 
                "Error", showButton: true);
        }
        finally
        {
            SpinnerService.Hide();
        }
    }

    public async Task ClientComboboxCreated()
    {
        await SetStickyClient();
        _clientDropdownInitialized = true;
    }

    private async Task SetStickyClient()
    {
        if (!AllowedBehavior.IsDpiUser)
        {
            return;
        }

        var selectedClientId = await LocalStorage!.GetItemAsync<long?>(StringConstants.HedgeDocumentSelectedClientId);

        if (selectedClientId > 0)
        {
            if (selectedClientId > 1)
            {
                var response = await Mediator.Send(new HasAdditionalService.Query(selectedClientId.Value, AddtlServices.HedgeAccounting),
                    CancellationToken.None);

                selectedClientId = response.HasAdditionalService ? selectedClientId : null;
                await LocalStorage!.SetItemAsync(StringConstants.HedgeDocumentSelectedClientId, selectedClientId);
            }

            SelectedClientId = selectedClientId;
        }
    }

    private async Task ClientComboboxValueChange()
    {
        await LocalStorage!.SetItemAsync(StringConstants.HedgeDocumentSelectedClientId, SelectedClientId);
    }

    private void CreateContentButtonClicked()
    {
        HideErrorMessages();
        _contentsComponent.IsCreateEditContentDialogVisible = true;
    }

    private void HideErrorMessages()
    {
        IsErrorMessageVisible = false;
        ErrorTitle = string.Empty;
        ErrorMessage = string.Empty;
    }

    private void ContentsListUpdated(int count)
    {
        _contentsListCount = count;
    }

    private void ShowActionFailed(ErrorMessage errorMessage)
    {
        IsErrorMessageVisible = true;
        ErrorTitle = errorMessage.Title;
        ErrorMessage = errorMessage.Message;
    }
}
