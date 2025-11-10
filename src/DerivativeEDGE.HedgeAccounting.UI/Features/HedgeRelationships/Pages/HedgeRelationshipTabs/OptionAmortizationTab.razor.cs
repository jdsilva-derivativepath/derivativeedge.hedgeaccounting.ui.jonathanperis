namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class OptionAmortizationTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> HedgeRelationshipOptionTimeValues { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>> HedgeRelationshipOptionTimeValuesChanged { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnEditOptionAmortization { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnDeleteOptionAmortization { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnDownloadExcelOptionAmortization { get; set; }
    [Parameter] public DerivativeEDGEDomainEntitiesEnumsSecurityType? HedgingItemSecurityType { get; set; }
    #endregion

    private DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM SelectedOptionAmortization { get; set; }
    private List<DerivativeEDGEHAApiViewModelsOptionAmortizationVM> OptionAmortizations { get; set; } = [];
    private List<DerivativeEDGEHAApiViewModelsOptionSwapletAmortizationVM> OptionSwapletAmortizations { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadOptionAmortizationGridList();
        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadOptionAmortizationGridList();
        StateHasChanged();
    }

    public async Task LoadOptionAmortizationGridList()
    {
        // Aggregate all option amortizations from all HedgeRelationshipOptionTimeValues
        if (HedgeRelationshipOptionTimeValues != null && HedgeRelationshipOptionTimeValues.Count > 0)
        {
            SelectedOptionAmortization = HedgeRelationshipOptionTimeValues.First();
            InitializeAmortizationCollections();
        }

        await Task.CompletedTask;
    }

    private void OnHedgeRelationshipOptionTimeValueRowClicked(DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM row)
    {
        SelectedOptionAmortization = row;
        InitializeAmortizationCollections();
    }

    private void InitializeAmortizationCollections()
    {
        // Clear both collections first
        OptionAmortizations = [];
        OptionSwapletAmortizations = [];

        if (SelectedOptionAmortization != null)
        {
            if (SelectedOptionAmortization.AmortizationMethod == DerivativeEDGEHAEntityEnumAmortizationMethod.Swaplet &&
                SelectedOptionAmortization.OptionTimeValueAmortType == DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.OptionIntrinsicValue)
            {
                if (SelectedOptionAmortization.OptionSwapletAmortizations != null && SelectedOptionAmortization.OptionSwapletAmortizations.Count > 0)
                {
                    OptionSwapletAmortizations = [.. SelectedOptionAmortization.OptionSwapletAmortizations];
                }
            }
            else
            {
                if (SelectedOptionAmortization.OptionAmortizations != null && SelectedOptionAmortization.OptionAmortizations.Count > 0) 
                {
                    OptionAmortizations = [.. SelectedOptionAmortization.OptionAmortizations];
                }
            }
        }
    }

    private async Task OnItemSelectedMatrix(MenuEventArgs args, DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM data)
    {
        switch (args.Item.Text)
        {
            case "Edit":
                if (OnEditOptionAmortization.HasDelegate)
                {
                    await OnEditOptionAmortization.InvokeAsync(data);
                }
                break;
            case "Delete":
                if (OnDeleteOptionAmortization.HasDelegate)
                {
                    await OnDeleteOptionAmortization.InvokeAsync(data);
                }
                break;
            case "Download Excel":
                if (OnDownloadExcelOptionAmortization.HasDelegate)
                {
                    await OnDownloadExcelOptionAmortization.InvokeAsync(data);
                }
                break;
        }
    }
}
