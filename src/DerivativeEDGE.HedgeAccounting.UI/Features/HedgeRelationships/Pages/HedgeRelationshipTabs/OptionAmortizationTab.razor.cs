using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class OptionAmortizationTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> HedgeRelationshipOptionTimeValues { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>> HedgeRelationshipOptionTimeValuesChanged { get; set; }
    #endregion

    public List<DerivativeEDGEHAApiViewModelsOptionAmortizationVM> OptionAmortizations { get; set; } = new();

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
            OptionAmortizations = HedgeRelationshipOptionTimeValues
                .Where(timeValue => timeValue.OptionAmortizations?.Any() == true)
                .SelectMany(timeValue => timeValue.OptionAmortizations)
                .OrderBy(amortization => amortization.PaymentDate)
                .ToList();
        }
        else
        {
            OptionAmortizations = new List<DerivativeEDGEHAApiViewModelsOptionAmortizationVM>();
        }

        await Task.CompletedTask;
    }

    private void OnItemSelectedMatrix(MenuEventArgs args, DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM data)
    {
        string concatenatedString = $"{args.Item.Text}: {data.ID}";
    }
}
