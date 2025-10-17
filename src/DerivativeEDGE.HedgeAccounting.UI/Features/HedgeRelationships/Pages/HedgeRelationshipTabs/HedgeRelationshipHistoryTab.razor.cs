namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class HedgeRelationshipHistoryTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM> HedgeRegressionBatches { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM>> HedgeRegressionBatchesChanged { get; set; }
    #endregion

    public List<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM> HistoryBatches { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        LoadHistoryData();
        await InvokeAsync(StateHasChanged);
    }

    private void LoadHistoryData()
    {
        if (HedgeRegressionBatches == null || !HedgeRegressionBatches.Any())
        {
            HistoryBatches = new List<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM>();
            return;
        }

        HistoryBatches = HedgeRegressionBatches
            .Where(batch => batch.Enabled)
            .OrderByDescending(batch => batch.CreatedOn)
            .ToList();
    }

    private string GetDisplayDate(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM batch)
    {
        // Try to get the date from HedgeRelationshipLog first
        if (batch.HedgeRelationshipLogs?.Any() == true)
        {
            var firstLog = batch.HedgeRelationshipLogs.First();
            if (firstLog.CreatedOn != DateTime.MinValue)
            {
                return firstLog.CreatedOn.ToString("MMMM dd, yyyy 'at' h:mm tt");
            }
        }

        // Try RunDate if it's a valid string and can be parsed
        if (!string.IsNullOrEmpty(batch.RunDate) && DateTime.TryParse(batch.RunDate, out DateTime runDate))
        {
            return runDate.ToString("MMMM dd, yyyy 'at' h:mm tt");
        }

        // Try ValueDate if it's a valid string and can be parsed
        if (!string.IsNullOrEmpty(batch.ValueDate) && DateTime.TryParse(batch.ValueDate, out DateTime valueDate))
        {
            return valueDate.ToString("MMMM dd, yyyy 'at' h:mm tt");
        }

        // Fallback to CreatedOn if it's not the default value
        if (batch.CreatedOn != DateTime.MinValue)
        {
            return batch.CreatedOn.ToString("MMMM dd, yyyy 'at' h:mm tt");
        }

        // Last resort - use current date
        return DateTime.Now.ToString("MMMM dd, yyyy 'at' h:mm tt");
    }
}
