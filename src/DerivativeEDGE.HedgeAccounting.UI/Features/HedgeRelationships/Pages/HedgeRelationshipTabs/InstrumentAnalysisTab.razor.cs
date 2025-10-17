namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class InstrumentAnalysisTab
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; }
    [Inject] private IJSRuntime JS { get; set; }
    #endregion

    #region Parameters
    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationshipChanged { get; set; }
    #endregion

    #region Public Properties
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM HedgedItem { get; set; } = new();
    public List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM> HedgedItems { get; set; } = new();
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM HedgingItem { get; set; } = new();
    public List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM> HedgingItems { get; set; } = new();
    public List<TradeDto> SelectExistingTrade { get; set; } = new();
    public bool IsSelectExistingTradeModal { get; set; }
    public bool IsLoadingTradeData { get; set; }
    #endregion

    #region Private Properties
    private List<HedgeCurrencyDropdownItem> Currency { get; set; } = new();
    private string ExistingTradeModalHeaderText { get; set; }
    #endregion

    #region Radio Button Properties
    // Radio button properties for mutual exclusivity
    private string RegressionType
    {
        get
        {
            if (HedgeRelationship?.CumulativeChanges == true)
                return "cumulative";
            else if (HedgeRelationship?.PeriodicChanges == true)
                return "periodic";
            else
                return null; // Return null when neither is selected
        }
        set
        {
            if (HedgeRelationship != null)
            {
                HedgeRelationship.CumulativeChanges = value == "cumulative";
                HedgeRelationship.PeriodicChanges = value == "periodic";
                _ = Task.Run(async () =>
                {
                    await UpdateParentData();
                    await InvokeAsync(StateHasChanged);
                }); // Fire and forget with state update
            }
        }
    }
    #endregion

    #region Constants
    private static readonly List<DropDownMenuItem> MenuItems = new()
    {
        new DropDownMenuItem { Text = "Callable Debt", Id = "callabledebt" },
        new DropDownMenuItem { Text = "Cancelable", Id = "cancelable" },
        new DropDownMenuItem { Text = "Cap Floor", Id = "cap" },
        new DropDownMenuItem { Text = "Collar", Id = "collar" },
        new DropDownMenuItem { Text = "Debt", Id = "debt" },
        new DropDownMenuItem { Text = "Debt Option", Id = "debtoption" },
        new DropDownMenuItem { Text = "Swap", Id = "swap" },
        new DropDownMenuItem { Text = "Swap With Cap/Floor", Id = "swapwithoption" },
        new DropDownMenuItem { Text = "Swaption", Id = "swaption" },
        new DropDownMenuItem { Text = "Corridor", Id = "corridor" },
        new DropDownMenuItem { Text = "FX Forward", Id = "fxforward" }
    };

    public class DropDownMenuItem
    {
        public string Text { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadCurrency();
        // Don't load instrument data here as HedgeRelationship parameter might not be set yet
    }

    // Commented for causing infinite loop issues
    // protected override async Task OnParametersSetAsync()
    // {
    //     await LoadInstrumentAnalysisData();
    // }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadInstrumentAnalysisData();
        }
    }
    #endregion

    #region Data Loading Methods
    private async Task LoadInstrumentAnalysisData()
    {
        if (HedgeRelationship != null)
        {
            // Load the data
            HedgedItems = HedgeRelationship.HedgedItems?.ToList() ?? new List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>();
            HedgingItems = HedgeRelationship.HedgingItems?.ToList() ?? new List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>();

            // Ensure mutual exclusivity - if both are true (shouldn't happen), prefer Cumulative Changes
            if (HedgeRelationship.CumulativeChanges && HedgeRelationship.PeriodicChanges)
            {
                HedgeRelationship.PeriodicChanges = false;
            }

            // Force UI update to refresh grids
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            // Initialize empty lists if HedgeRelationship is null
            HedgedItems = new List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>();
            HedgingItems = new List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadCurrency()
    {
        var response = await Mediator.Send(new GetHedgeRelationshipCurrencyList.Query());
        response.Currency.Insert(0, new HedgeCurrencyDropdownItem { LongName = "None", ShortName = "" }); // Add the "None" option
        Currency = response.Currency;

        // Check if current ReportCurrency exists in the response, if not set to "None"
        if (HedgeRelationship != null)
        {
            // If ReportCurrency is null, empty, or not found in the currency list, set to "None" (empty string)
            if (string.IsNullOrWhiteSpace(HedgeRelationship.ReportCurrency) ||
                !Currency.Any(c => c.ShortName == HedgeRelationship.ReportCurrency))
            {
                HedgeRelationship.ReportCurrency = ""; // Represents "None"
            }
        }
    }

    private async Task LoadHedgeTradeList()
    {
        IsLoadingTradeData = true;

        try
        {
            var query = new GetTradeDataSource.Query
            {
                ClientId = HedgeRelationship.ClientID,
                BankEntityId = HedgeRelationship.BankEntityID
            };

            var response = await Mediator.Send(query);

            if (response.IsSuccess)
            {
                SelectExistingTrade = response.TradeData ?? new List<TradeDto>();
            }
            else
            {
                // Handle error - you might want to show a toast/alert here
                Console.WriteLine($"Error loading trade data: {response.ErrorMessage}");
                SelectExistingTrade = new List<TradeDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception loading trade data: {ex.Message}");
            SelectExistingTrade = new List<TradeDto>();
        }
        finally
        {
            IsLoadingTradeData = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CloseSelectExistingTradeModal()
    {
        IsSelectExistingTradeModal = false;
        SelectExistingTrade = new List<TradeDto>(); // Clear data when closing
        IsLoadingTradeData = false; // Reset loading state
    }

    /// <summary>
    /// Public method to force refresh of grid data - can be called when tab becomes visible
    /// </summary>
    public async Task RefreshGridData()
    {
        await LoadInstrumentAnalysisData();
    }
    #endregion
    #region Event Handlers
    private async Task UpdateParentData()
    {
        if (HedgeRelationship != null)
        {
            HedgeRelationship.HedgedItem = HedgedItem;
            HedgeRelationship.HedgedItems = HedgedItems ?? new List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>();
            HedgeRelationship.HedgingItem = HedgingItem;
            HedgeRelationship.HedgingItems = HedgingItems ?? new List<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>();
            await HedgeRelationshipChanged.InvokeAsync(HedgeRelationship);
        }
    }

    // Grid event handlers for data synchronization
    public async Task OnHedgedItemChanged()
    {
        await UpdateParentData();
        await InvokeAsync(StateHasChanged);
    }

    public async Task OnHedgingItemChanged()
    {
        await UpdateParentData();
        await InvokeAsync(StateHasChanged);
    }

    // Method to handle form field changes (effectiveness settings, etc.)
    public async Task OnEffectivenessSettingsChanged(object args = null)
    {
        await UpdateParentData();
        await InvokeAsync(StateHasChanged);
    }

    // Specific handler for reporting frequency changes
    private async Task OnReportingFrequencyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<DerivativeEDGEHAEntityEnumReportingFrequency?, ReportFrequencyDropdownModel> args)
    {
        if (HedgeRelationship != null)
        {
            HedgeRelationship.ReportingFrequency = args.Value.GetValueOrDefault();
            await UpdateParentData();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SelectExistingTradeHandler(string hedgeType)
    {
        ExistingTradeModalHeaderText = hedgeType == "HedgeItem" ? "Hedged" : "Hedging";

        // Open modal immediately with empty data and loading state
        SelectExistingTrade = new List<TradeDto>(); // Start with empty data
        IsSelectExistingTradeModal = true;

        // Load data asynchronously after modal is shown for both hedge types
        _ = Task.Run(async () => await LoadHedgeTradeList());
    }

    private async Task RemoveHedgedItem(DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM item)
    {
        if (item != null && HedgedItems != null)
        {
            HedgedItems.Remove(item);
            HedgedItems = HedgedItems.ToList(); // Force collection refresh
            await UpdateParentData();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task RemoveHedgingItem(DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM item)
    {
        if (item != null && HedgingItems != null)
        {
            HedgingItems.Remove(item);
            HedgingItems = HedgingItems.ToList(); // Force collection refresh
            await UpdateParentData();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LinkTradeToHedging(TradeDto trade)
    {
        try
        {
            var query = new GetTradesForHedging.Query(trade.Id, HedgeRelationship.ClientID);
            var response = await Mediator.Send(query);

            var hedgeItemVM = response.HedgeItem;

            if (hedgeItemVM is not null)
            {
                if (ExistingTradeModalHeaderText == "Hedged")
                {
                    hedgeItemVM.HedgeRelationshipID = HedgeRelationship.ID;
                    hedgeItemVM.HedgeRelationshipItemType = DerivativeEDGEHAEntityEnumHedgeRelationshipItemType.HedgedItem;

                    HedgedItem = hedgeItemVM;
                    HedgedItems.Add(hedgeItemVM);
                    HedgedItems = HedgedItems.ToList(); // Force collection refresh
                }
                else if (ExistingTradeModalHeaderText == "Hedging")
                {
                    hedgeItemVM.HedgeRelationshipID = HedgeRelationship.ID;
                    hedgeItemVM.HedgeRelationshipItemType = DerivativeEDGEHAEntityEnumHedgeRelationshipItemType.HedgingItem;

                    HedgingItem = hedgeItemVM;
                    HedgingItems.Add(hedgeItemVM);
                    HedgingItems = HedgingItems.ToList(); // Force collection refresh
                }

                await UpdateParentData();
                CloseSelectExistingTradeModal();
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                Console.WriteLine("Error: No hedge item was returned from the service.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception linking trade: {ex.Message}");
        }
    }

    private DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM ConvertToHedgedItem(DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM item)
    {
        return new DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM
        {
            ID = item.ID,
            HedgeRelationshipItemLegs = item.HedgeRelationshipItemLegs,
            HedgeRelationshipItemType = DerivativeEDGEHAEntityEnumHedgeRelationshipItemType.HedgedItem,
            ItemID = item.ItemID,
            Description = item.Description,
            Notional = item.Notional,
            Spread = item.Spread,
            Rate = item.Rate,
            EffectiveDate = item.EffectiveDate,
            MaturityDate = item.MaturityDate,
            ItemStatus = item.ItemStatus,
            ItemStatusText = item.ItemStatusText,
            SecurityType = item.SecurityType
        };
    }

    private DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM ConvertToHedgingItem(DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM item)
    {
        return new DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM
        {
            ID = item.ID,
            HedgeRelationshipItemLegs = item.HedgeRelationshipItemLegs,
            HedgeRelationshipItemType = DerivativeEDGEHAEntityEnumHedgeRelationshipItemType.HedgingItem,
            ItemID = item.ItemID,
            Description = item.Description,
            Notional = item.Notional,
            Spread = item.Spread,
            Rate = item.Rate,
            EffectiveDate = item.EffectiveDate,
            MaturityDate = item.MaturityDate,
            ItemStatus = item.ItemStatus,
            ItemStatusText = item.ItemStatusText,
            SecurityType = item.SecurityType
        };
    }
    #endregion

    #region Helper Methods
    public IEnumerable<DropdownModel> GetDropdownDatasource(string dataSet = "entity")
    {
        return dataSet.ToLower() switch
        {
            "assessmentmethod" => GetAssessmentMethodOptions(),
            "periodsize" => GetPeriodSizeOptions(),
            _ => throw new ArgumentException($"Invalid data set: {dataSet}")
        };
    }

    private static IEnumerable<DropdownModel> GetAssessmentMethodOptions()
    {
        return new List<DropdownModel>
        {
            new() { ID = 0, Text = "None" },
            new() { ID = 1, Text = "Regression - Change in Fair Value" },
            new() { ID = 2, Text = "Dollar Offset" },
            new() { ID = 3, Text = "Cummulative Dollar Offset" },
            new() { ID = 4, Text = "Index Regression" },
            new() { ID = 5, Text = "Cumulative Index Regression" },
            new() { ID = 6, Text = "Scenario Analysis" },
            new() { ID = 7, Text = "Scenario Regression" },
            new() { ID = 9, Text = "Cumulative Dollar Offset" },
        };
    }

    private static IEnumerable<ReportFrequencyDropdownModel> GetReportFrequencyOptions()
    {
        return new List<ReportFrequencyDropdownModel>
        {
            new() { Value = null, Text = "None" },
            new() { Value = DerivativeEDGEHAEntityEnumReportingFrequency.Monthly, Text = "Monthly" },
            new() { Value = DerivativeEDGEHAEntityEnumReportingFrequency.Quarterly, Text = "Quarterly" }
        };
    }

    private static IEnumerable<DropdownModel> GetPeriodSizeOptions()
    {
        return new List<DropdownModel>
        {
            new() { Value = "None", Text = "None" },
            new() { Value = "Week", Text = "Week" },
            new() { Value = "Month", Text = "Month" },
            new() { Value = "Quarter", Text = "Quarter" },
        };
    }
    #endregion

    #region Trade Management Methods
    private void OnNewTradeMenuItemSelected(MenuEventArgs args)
    {
        var url = GetNewTradeUrl(args.Item.Id);
        if (!string.IsNullOrEmpty(url))
        {
            OpenUrlInNewTab(url);
        }
    }

    private void OpenUrlInNewTab(string url)
    {
        // Use JS interop to open a new tab
        JS.InvokeVoidAsync("window.open", url, "_blank");
    }

    private string GetNewTradeUrl(string type)
    {
        var clientId = HedgeRelationship?.ClientID.ToString() ?? "0";
        return type.ToLower() switch
        {
            "callabledebt" => $"/CallableDebt/Add?cId={clientId}",
            "cancelable" => $"/Cancelable/Add?cId={clientId}",
            "cap" => $"/CapFloor/AddCapFloor?clientId={clientId}",
            "collar" => $"/Collar/Add?cId={clientId}",
            "debt" => $"/Debt/AddDebt?cId={clientId}",
            "debtoption" => $"/DebtOption/Add?cId={clientId}",
            "swap" => $"/Swap/AddSwap?cId={clientId}",
            "swapwithoption" => $"/SwapEmbeddedOption/Add?cId={clientId}",
            "swaption" => $"/Swaption/AddSwaption?cId={clientId}",
            "corridor" => $"/Corridor/Add?cId={clientId}",
            "fxforward" => $"/FxSingle/Add?cId={clientId}&type=11",
            _ => null
        };
    }
    #endregion

    #region Models
    public class DropdownModel
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class ReportFrequencyDropdownModel
    {
        public DerivativeEDGEHAEntityEnumReportingFrequency? Value { get; set; }
        public string Text { get; set; }
    }

    // Property to track the current item type for trade operations
    private string CurrentHedgeRelationshipItemType { get; set; } = "HedgedItem";
    #endregion
}
