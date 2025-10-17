using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class AmortizationTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> HedgeRelationshipOptionTimeValueAmorts { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>> HedgeRelationshipOptionTimeValueAmortsChanged { get; set; }
    #endregion

    public List<DerivativeEDGEHAApiViewModelsOptionTimeValueAmortRollScheduleVM> OptionTimeValueAmortRollSchedules { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadAmortizationGridList();
        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadAmortizationGridList();
        StateHasChanged();
    }

    private async Task LoadAmortizationGridList()
    {
        // Aggregate all roll schedules from all HedgeRelationshipOptionTimeValueAmorts
        if (HedgeRelationshipOptionTimeValueAmorts?.Any() == true)
        {
            OptionTimeValueAmortRollSchedules = HedgeRelationshipOptionTimeValueAmorts
                .Where(amort => amort.OptionTimeValueAmortRollSchedules?.Any() == true)
                .SelectMany(amort => amort.OptionTimeValueAmortRollSchedules)
                .OrderBy(schedule => schedule.PaymentDate)
                .ToList();
        }
        else
        {
            OptionTimeValueAmortRollSchedules = new List<DerivativeEDGEHAApiViewModelsOptionTimeValueAmortRollScheduleVM>();
        }

        await Task.CompletedTask;
    }
    private void OnItemSelectedMatrix(MenuEventArgs args, DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM data)
    {
        string concatenatedString = $"{args.Item.Text}: {data.ID}";
    }
}
