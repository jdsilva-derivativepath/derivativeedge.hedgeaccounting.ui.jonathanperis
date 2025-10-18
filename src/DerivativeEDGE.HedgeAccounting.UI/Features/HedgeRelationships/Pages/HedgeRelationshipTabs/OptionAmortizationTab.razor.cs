namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class OptionAmortizationTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> HedgeRelationshipOptionTimeValues { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>> HedgeRelationshipOptionTimeValuesChanged { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnEditOptionAmortization { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnDeleteOptionAmortization { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnDownloadExcelOptionAmortization { get; set; }
    #endregion

    public List<DerivativeEDGEHAApiViewModelsOptionAmortizationVM> OptionAmortizations { get; set; } = [];

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

    private async Task LoadOptionAmortizationGridList()
    {
        // Aggregate all option amortizations from all HedgeRelationshipOptionTimeValues
        if (HedgeRelationshipOptionTimeValues?.Any() == true)
        {
            OptionAmortizations = [.. HedgeRelationshipOptionTimeValues
                .Where(timeValue => timeValue.OptionAmortizations?.Any() == true)
                .SelectMany(timeValue => timeValue.OptionAmortizations)
                .OrderBy(amortization => amortization.PaymentDate)];
        }
        else
        {
            OptionAmortizations = [];
        }

        await Task.CompletedTask;
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
