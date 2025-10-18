namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class HedgeRelationshipLogsTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM> HedgeRegressionBatches { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM>> HedgeRegressionBatchesChanged { get; set; }
    #endregion

    #region Public Properties
    public List<EnhancedLogDetail> Logs { get; set; } = [];
    #endregion

    #region Models
    public class EnhancedLogDetail : DerivativeEDGEHAApiViewModelsHedgeRelationshipLogVm
    {
        public int Success { get; set; }
        public int Errors { get; set; }
    }
    #endregion

    protected override async Task OnParametersSetAsync()
    {
        HedgeRegressionBatchesLogsData();
        await InvokeAsync(StateHasChanged);
    }

    #region Data Loading Methods
    private void HedgeRegressionBatchesLogsData()
    {
        if (HedgeRegressionBatches == null || !HedgeRegressionBatches.Any())
        {
            Logs = [];
            return;
        }

        var allLogs = new List<EnhancedLogDetail>();

        foreach (var batch in HedgeRegressionBatches)
        {
            if (batch.HedgeRelationshipLogs?.Any() == true) // Check HedgeRelationshipLog.Logs
            {
                foreach (var logDetail in batch.HedgeRelationshipLogs)
                {
                    allLogs.Add(new EnhancedLogDetail
                    {
                        ID = logDetail.ID,
                        HedgeRegressionBatchID = logDetail.HedgeRegressionBatchID,
                        HedgeRelationshipID = logDetail.HedgeRelationshipID,
                        Key = logDetail.Key,
                        Level = logDetail.Level,
                        Message = logDetail.Message,
                        Enabled = logDetail.Enabled,
                        CreatedOn = logDetail.CreatedOn,
                        CreatedByID = logDetail.CreatedByID,
                        ModifiedOn = logDetail.ModifiedOn,
                        ModifiedByID = logDetail.ModifiedByID,
                        Success = batch.HedgeRelationshipLog.Success,
                        Errors = batch.HedgeRelationshipLog.Errors
                    });
                }
            }

            if (batch.HedgeRelationshipLogs?.Any() == true) // Check HedgeRelationshipLogs (multiple logs per batch)
            {
                foreach (var logContainer in batch.HedgeRelationshipLogs)
                {
                    //TODO: Revisit this logic later to see if we want to flatten nested logs or not

                    // if (logContainer.Logs?.Any() == true)
                    // {
                    //     foreach (var logDetail in logContainer.Logs)
                    //     {
                    //         allLogs.Add(new EnhancedLogDetail
                    //         {
                    //             ID = logDetail.ID,
                    //             HedgeRegressionBatchID = logDetail.HedgeRegressionBatchID,
                    //             HedgeRelationshipID = logDetail.HedgeRelationshipID,
                    //             Key = logDetail.Key,
                    //             Level = logDetail.Level,
                    //             Message = logDetail.Message,
                    //             Enabled = logDetail.Enabled,
                    //             CreatedOn = logDetail.CreatedOn,
                    //             CreatedByID = logDetail.CreatedByID,
                    //             ModifiedOn = logDetail.ModifiedOn,
                    //             ModifiedByID = logDetail.ModifiedByID,
                    //             // Success = logContainer.Success,
                    //             // Errors = logContainer.Errors
                    //         });
                    //     }
                    // }
                    // else
                    // {
                    //     allLogs.Add(new EnhancedLogDetail // If no nested logs, treat the log container itself as a log entry
                    //     {
                    //         ID = logContainer.ID,
                    //         HedgeRegressionBatchID = logContainer.HedgeRegressionBatchID,
                    //         HedgeRelationshipID = logContainer.HedgeRelationshipID,
                    //         Key = logContainer.Key,
                    //         Level = logContainer.Level,
                    //         Message = logContainer.Message,
                    //         Enabled = logContainer.Enabled,
                    //         CreatedOn = logContainer.CreatedOn,
                    //         CreatedByID = logContainer.CreatedByID,
                    //         ModifiedOn = logContainer.ModifiedOn,
                    //         ModifiedByID = logContainer.ModifiedByID,
                    //         // Success = logContainer.Success,
                    //         // Errors = logContainer.Errors
                    //     });
                    // }

                    allLogs.Add(new EnhancedLogDetail // If no nested logs, treat the log container itself as a log entry
                    {
                        ID = logContainer.ID,
                        HedgeRegressionBatchID = logContainer.HedgeRegressionBatchID,
                        HedgeRelationshipID = logContainer.HedgeRelationshipID,
                        Key = logContainer.Key,
                        Level = logContainer.Level,
                        Message = logContainer.Message,
                        Enabled = logContainer.Enabled,
                        CreatedOn = logContainer.CreatedOn,
                        CreatedByID = logContainer.CreatedByID,
                        ModifiedOn = logContainer.ModifiedOn,
                        ModifiedByID = logContainer.ModifiedByID,
                        // Success = logContainer.Success,
                        // Errors = logContainer.Errors
                    });
                }
            }
        }

        // Sort by creation date (newest first) and remove duplicates
        Logs = [.. allLogs
            .GroupBy(log => new { log.ID, log.Key, log.Message })
            .Select(group => group.First())
            .OrderByDescending(log => log.CreatedOn)];
    }
    #endregion
}
