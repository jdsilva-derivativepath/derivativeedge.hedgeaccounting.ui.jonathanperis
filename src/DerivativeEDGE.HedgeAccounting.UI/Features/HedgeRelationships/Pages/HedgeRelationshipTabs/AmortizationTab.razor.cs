namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class AmortizationTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> HedgeRelationshipOptionTimeValueAmorts { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>> HedgeRelationshipOptionTimeValueAmortsChanged { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnEditAmortization { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnDeleteAmortization { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM> OnDownloadExcelAmortization { get; set; }
    #endregion

    public List<DerivativeEDGEHAApiViewModelsOptionTimeValueAmortRollScheduleVM> OptionTimeValueAmortRollSchedules { get; set; } = [];

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

    public async Task LoadAmortizationGridList()
    {
        // Aggregate all roll schedules from all HedgeRelationshipOptionTimeValueAmorts
        if (HedgeRelationshipOptionTimeValueAmorts?.Any() == true)
        {
            OptionTimeValueAmortRollSchedules = [.. HedgeRelationshipOptionTimeValueAmorts
                .Where(amort => amort.OptionTimeValueAmortRollSchedules?.Any() == true)
                .SelectMany(amort => amort.OptionTimeValueAmortRollSchedules)
                .OrderBy(schedule =>
                {
                    // Parse StartDate as DateTime for correct ordering
                    _ = DateTime.TryParse(schedule.StartDate, out DateTime parsedDate);
                    return parsedDate;
                })];
        }
        else
        {
            OptionTimeValueAmortRollSchedules = [];
        }

        await Task.CompletedTask;
    }
    private async Task OnItemSelectedMatrix(MenuEventArgs args, DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM data)
    {
        switch (args.Item.Text)
        {
            case "Edit":
                if (OnEditAmortization.HasDelegate)
                {
                    await OnEditAmortization.InvokeAsync(data);
                }
                break;
            case "Delete":
                if (OnDeleteAmortization.HasDelegate)
                {
                    await OnDeleteAmortization.InvokeAsync(data);
                }
                break;
            case "Download Excel":
                if (OnDownloadExcelAmortization.HasDelegate)
                {
                    await OnDownloadExcelAmortization.InvokeAsync(data);
                }
                break;
        }
    }
}
