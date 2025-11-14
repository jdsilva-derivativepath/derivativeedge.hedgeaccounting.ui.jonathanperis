using Syncfusion.Blazor.Grids;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class TestResultsTab
{
    #region Constants
    private const string CHART_BACKGROUND_COLOR = "#66666e";
    private const string CHART_PLOT_BACKGROUND_COLOR = "#4e4e55";
    private const string CHART_GRID_COLOR = "#f1f1f1";
    private const string CHART_SCATTER_COLOR = "#87bfe3";
    private const string CHART_TRENDLINE_COLOR = "#ff6600";
    private const string STAT_PASS_CLASS = "stat-pass";
    private const string STAT_FAIL_CLASS = "stat-fail";
    private const string NOT_AVAILABLE = "N/A";
    private const int CHART_HEIGHT = 400;
    private const int CHART_MARKER_SIZE = 10;
    private const int TRENDLINE_WIDTH = 2;
    #endregion

    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM> HedgeRegressionBatches { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM>> HedgeRegressionBatchesChanged { get; set; }

    // *** ADD THIS NEW PARAMETER ***
    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM LatestHedgeRegressionBatch { get; set; }

    /// <summary>
    /// Parent hedge relationship for permission checks and API operations
    /// </summary>
    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationshipChanged { get; set; }

    /// <summary>
    /// Curve date selected by user for API operations (matching legacy Model.ValueDate)
    /// </summary>
    [Parameter] public DateTime? CurveDate { get; set; }
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; }
    [Inject] private ILogger<TestResultsTab> Logger { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IAlertService AlertService { get; set; }
    [Inject] private IUserAuthData UserAuthData { get; set; }
    #endregion

    #region Private Fields
    private bool _disposed = false;
    private bool _isDeleting = false;
    private bool _isDownloading = false;
    private bool _showDeleteConfirmation = false;
    private DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM _batchToDelete = null;
    private DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM _selectedBatch = null;
    #endregion

    #region Public Properties
    public List<ChartDataModel> ChartData { get; set; } = [];
    public List<ChartDataModel> TrendlineData { get; set; } = [];
    public double ChartMinValue { get; set; } = 0;
    public double ChartMaxValue { get; set; } = 0;
    #endregion

    private DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM LatestBatch =>
        _selectedBatch ??
    LatestHedgeRegressionBatch ??
    HedgeRegressionBatches?.FirstOrDefault(x => x.Enabled);

    #region Lifecycle Methods
    protected override void OnInitialized()
    {
        try
        {
            Logger?.LogInformation("TestResultsTab initialized");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error initializing TestResultsTab");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_disposed) return;

        Logger?.LogInformation("OnParametersSetAsync called");
        Logger?.LogInformation($"LatestHedgeRegressionBatch: {LatestHedgeRegressionBatch != null}");
        Logger?.LogInformation($"HedgeRegressionBatches count: {HedgeRegressionBatches?.Count ?? 0}");

        await LoadData();
        StateHasChanged();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                Logger?.LogInformation("TestResultsTab disposed");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error disposing TestResultsTab");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Public method to refresh test results data - called when new regression data is available
    /// </summary>
    public async Task RefreshTestResultsData()
    {
        if (_disposed) return;

        try
        {
            Logger?.LogInformation("Refreshing test results data...");

            await InvokeAsync(() =>
            {
                GenerateChartData();
                StateHasChanged();
            });

            Logger?.LogInformation("Test results data refreshed successfully");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error refreshing test results data");
        }
    }

    // Add this method if you need JS interop
    [JSInvokable]
    public async Task OnChartRendered()
    {
        if (_disposed) return;

        try
        {
            Logger?.LogInformation("Chart rendered successfully");
            // Add any post-render logic here if needed
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in OnChartRendered");
        }
    }
    #endregion

    #region Private Methods
    private async Task LoadData()
    {
        if (_disposed) return;

        try
        {
            GenerateChartData();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error loading data");
        }
    }

    private void GenerateChartData()
    {
        if (_disposed) return;

        try
        {
            Logger?.LogInformation($"Generating chart data. LatestBatch is null: {LatestBatch == null}");

            if (LatestBatch?.HedgeRegressionBatchResults?.Any() != true)
            {
                Logger?.LogWarning("No regression batch results available for chart generation");
                Logger?.LogInformation($"LatestHedgeRegressionBatch: {LatestHedgeRegressionBatch != null}");
                Logger?.LogInformation($"HedgeRegressionBatches count: {HedgeRegressionBatches?.Count ?? 0}");
                Logger?.LogInformation($"HedgeRegressionBatches with results: {HedgeRegressionBatches?.Count(b => b.HedgeRegressionBatchResults?.Any() == true) ?? 0}");

                ResetChartData();
                return;
            }

            Logger?.LogInformation($"Found {LatestBatch.HedgeRegressionBatchResults.Count} regression batch results");

            // Check if all adjusted values are zeros (matching original logic)
            var allZeros = LatestBatch.HedgeRegressionBatchResults.All(item => item.AdjustedValue == 0);

            var graphData = new List<ChartDataModel>();
            var xValues = new List<double>();
            var yValues = new List<double>();

            foreach (var item in LatestBatch.HedgeRegressionBatchResults)
            {
                var xValue = (double)item.HedgedFairValueChanged;
                var yValue = allZeros ? (double)item.HedgingFairValueChanged : (double)item.AdjustedValue;

                graphData.Add(new ChartDataModel
                {
                    XValue = xValue,
                    YValue = yValue
                });

                xValues.Add(xValue);
                yValues.Add(yValue);
            }

            ChartData = graphData;
            SetChartAxisLimits(xValues, yValues);
            GenerateTrendlineData(graphData);

            Logger?.LogInformation($"Generated chart data with {ChartData.Count} points. AllZeros: {allZeros}");
            Logger?.LogInformation($"Chart axis limits: Min={ChartMinValue:F2}, Max={ChartMaxValue:F2}");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error generating chart data");
            ResetChartData();
        }
    }

    private void SetChartAxisLimits(List<double> xValues, List<double> yValues)
    {
        if (_disposed) return;

        if (xValues.Count != 0 && yValues.Count != 0)
        {
            var minXY = Math.Min(xValues.Min(), yValues.Min());
            var maxXY = Math.Max(xValues.Max(), yValues.Max());

            // Add some padding to the chart limits
            var range = maxXY - minXY;
            var padding = range * 0.1; // 10% padding

            ChartMinValue = minXY - padding;
            ChartMaxValue = maxXY + padding;
        }
        else
        {
            ChartMinValue = 0;
            ChartMaxValue = 0;
        }
    }

    private void GenerateTrendlineData(List<ChartDataModel> graphData)
    {
        if (_disposed) return;

        if (graphData.Count > 1 && LatestBatch != null)
        {
            var slope = LatestBatch.Slope;
            var intercept = LatestBatch.YIntercept;

            // Sort data by X value for proper line rendering
            var sortedData = graphData.OrderBy(d => d.XValue).ToList();

            TrendlineData =
            [
                new() { XValue = sortedData.First().XValue, YValue = slope * sortedData.First().XValue + intercept },
                new() { XValue = sortedData.Last().XValue, YValue = slope * sortedData.Last().XValue + intercept }
            ];
        }
        else
        {
            TrendlineData = [];
        }
    }

    private void ResetChartData()
    {
        if (_disposed) return;

        ChartData = [];
        TrendlineData = [];
        ChartMinValue = 0;
        ChartMaxValue = 0;
    }
    #endregion

    #region Event Handlers
    private void UpdateTable(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM selectedBatch)
    {
        try
        {
            if (selectedBatch.HedgeRegressionBatchResults.Count != null || selectedBatch.HedgeRegressionBatchResults.Count != 0)
            {
                var result = HedgeRegressionBatches.Where(x => x.ID == selectedBatch.ID).FirstOrDefault();
                if (result != null)
                {
                    LatestBatch.HedgeRegressionBatchResults = result.HedgeRegressionBatchResults;
                }
                else
                {
                    Logger.LogInformation($"No matching batch results found for batch ID: {selectedBatch.ID}");
                }

            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"Error occurred while handling UpdateTable row selection for batch {selectedBatch?.ID}");
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles row click/selection in the "All Tests" grid
    /// </summary>
    private async Task OnRowSelected(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM selectedBatch)
    {
        if (_disposed || selectedBatch == null) return;

        try
        {
            Logger?.LogInformation($"Test batch selected: ID={selectedBatch.ID}, RunDate={selectedBatch.RunDate}");

            _selectedBatch = selectedBatch;

            // Regenerate chart data with the selected batch
            await InvokeAsync(() =>
                   {
                       GenerateChartData();
                       UpdateTable(selectedBatch);
                       StateHasChanged();
                   });

            Logger?.LogInformation("Chart and statistics updated with selected batch");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"Error handling row selection for batch {selectedBatch?.ID}");
        }
    }

    private async Task OnItemSelectedMatrix(MenuEventArgs args, DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM data)
    {
        if (_disposed) return;

        try
        {
            Logger?.LogInformation($"Menu item '{args.Item.Text}' selected for batch ID: {data.ID}");

            switch (args.Item.Text)
            {
                case "Download Excel":
                    await HandleExcelDownload(data);
                    break;
                case "Delete":
                    await HandleDeleteRequest(data);
                    break;
                default:
                    Logger?.LogWarning($"Unknown menu action: {args.Item.Text}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"Error handling menu action '{args.Item.Text}' for batch {data.ID}");
        }
    }

    /// <summary>
    /// Handles Excel download action for a test batch.
    /// Legacy: hr_hedgeRelationshipAddEditCtrl.js -> selectedItemActionTestChanged() -> 'Download Excel'
    /// </summary>
    private async Task HandleExcelDownload(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM data)
    {
        if (_disposed || HedgeRelationship == null) return;

        try
        {
            _isDownloading = true;
            StateHasChanged();

            Logger?.LogInformation($"Downloading Excel for batch ID: {data.ID}");

            var query = new DownloadTestResultExcelService.Query(data.ID, HedgeRelationship, CurveDate);
            var result = await Mediator.Send(query);

            // Use DotNetStreamReference for proper binary file download
            using var streamRef = new DotNetStreamReference(stream: result.ExcelStream);
            await JSRuntime.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);

            await AlertService.ShowToast("Test results downloaded successfully!", AlertKind.Success, "Success", showButton: true);
        }
        catch (ArgumentNullException)
        {
            await AlertService.ShowToast("Hedge relationship data is required", AlertKind.Error, "Error", showButton: true);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"Failed to download Excel for batch {data.ID}");
            await AlertService.ShowToast($"Failed to download Excel: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            _isDownloading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Shows confirmation dialog for delete action.
    /// Legacy: hr_hedgeRelationshipAddEditCtrl.js -> selectedItemActionTestChanged() -> 'Delete' -> confirm()
    /// </summary>
    private async Task HandleDeleteRequest(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM data)
    {
        if (_disposed) return;

        _batchToDelete = data;
        _showDeleteConfirmation = true;
        StateHasChanged();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles confirmed delete action for a test batch.
    /// Legacy: hr_hedgeRelationshipAddEditCtrl.js -> setModelData(response.data) after successful delete
    /// </summary>
    private async Task HandleDeleteConfirmed()
    {
        if (_disposed || _batchToDelete == null || HedgeRelationship == null) return;

        try
        {
            _isDeleting = true;
            _showDeleteConfirmation = false;
            StateHasChanged();

            Logger?.LogInformation($"Deleting batch ID: {_batchToDelete.ID}");

            var command = new DeleteTestBatchService.Command(_batchToDelete.ID, HedgeRelationship);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                // Update the parent hedge relationship with the response (legacy: setModelData(response.data))
                await HedgeRelationshipChanged.InvokeAsync(result.UpdatedHedgeRelationship);

                // Update the HedgeRegressionBatches parameter to refresh the "All Tests" grid
                await HedgeRegressionBatchesChanged.InvokeAsync(result.UpdatedHedgeRelationship.HedgeRegressionBatches);

                await AlertService.ShowToast("Test batch deleted successfully!", AlertKind.Success, "Success", showButton: true);
            }
            else
            {
                await AlertService.ShowToast("Failed to delete test batch", AlertKind.Error, "Error", showButton: true);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"Failed to delete batch {_batchToDelete?.ID}");
            await AlertService.ShowToast($"Failed to delete test batch: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            _isDeleting = false;
            _batchToDelete = null;
            StateHasChanged();
        }
    }

    private void HandleDeleteCancelled()
    {
        _showDeleteConfirmation = false;
        _batchToDelete = null;
        StateHasChanged();
    }

    private bool IsIntrinsicValues()
    {
        return HedgeRelationship?.ProspectiveEffectivenessMethodID == 11
               && HedgeRelationship?.RetrospectiveEffectivenessMethodID == 11;
    }

    private bool ShouldShowTimeValueColumns()
    {
        // Primary condition: Hide if OffMarket is false
        if (HedgeRelationship?.OffMarket != true)
        {
            return false;
        }

        // Secondary condition: Hide if all OptionTimeValue entries are zero
        var results = LatestBatch?.HedgeRegressionBatchResults;
        if (results == null || !results.Any())
        {
            return false;
        }

        // Check if all OptionTimeValue entries are zero
        bool allTimeValuesAreZero = results.All(r => r.OptionTimeValue == 0);

        return !allTimeValuesAreZero;
    }

    private async void OnQueryCellInfoHandler(QueryCellInfoEventArgs<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchResultVM> args)
    {
        // Apply background color to Change in Fair Values columns
        if (args.Column.Field == nameof(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchResultVM.HedgedFairValueChanged) ||
            args.Column.Field == nameof(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchResultVM.HedgingFairValueChanged))
        {
            args.Cell.AddStyle(new string[] { "background-color: rgb(225, 240, 249);" });
        }
    }
    #endregion

    #region Helper Methods
    // Helper methods for displaying statistical values with enhanced null safety
    private string GetSlopeValue() =>
        _disposed ? NOT_AVAILABLE : (LatestBatch?.Slope != null ? Math.Round(LatestBatch.Slope, 2).ToString("F2") : NOT_AVAILABLE);

    private string GetRValue() =>
        _disposed ? NOT_AVAILABLE : (LatestBatch?.R != null ? Math.Round(LatestBatch.R, 2).ToString("F2") : NOT_AVAILABLE);

    private string GetRSquaredValue() =>
        _disposed ? NOT_AVAILABLE : (LatestBatch?.RSquared != null ? Math.Round(LatestBatch.RSquared, 2).ToString("F2") : NOT_AVAILABLE);

    private string GetStandardErrorValue() =>
        _disposed ? NOT_AVAILABLE : (LatestBatch?.StandardError != null ? Math.Round(LatestBatch.StandardError, 2).ToString("F2") : NOT_AVAILABLE);

    private string GetObservationValue()
    {
        if (_disposed || LatestBatch?.HedgeRegressionBatchResults == null) return NOT_AVAILABLE;

        // Count the number of observation events (true values)
        var observationCount = LatestBatch.HedgeRegressionBatchResults.Count(x => x.ObservationEvent);
        return observationCount.ToString();
    }

    private string GetYInterceptValue() =>
        _disposed ? NOT_AVAILABLE : (LatestBatch?.YIntercept != null ? Math.Round(LatestBatch.YIntercept, 2).ToString("F2") : NOT_AVAILABLE);

    private string GetTTestValue()
    {
        if (_disposed || LatestBatch == null) return NOT_AVAILABLE;

        var tTestValue = Math.Round(LatestBatch.B1tStat, 2).ToString("N2");
        var analytics = LatestBatch.B1tStatAnalytics ?? "";
        var statusClass = analytics.Equals("pass", StringComparison.OrdinalIgnoreCase) ? STAT_PASS_CLASS : STAT_FAIL_CLASS;

        return $"{tTestValue}/<span class=\"{statusClass}\">{analytics}</span>";
    }

    private string GetFStatValue()
    {
        if (_disposed || LatestBatch == null) return NOT_AVAILABLE;

        var fStatValue = Math.Round(LatestBatch.FStat, 2).ToString("N2");
        var analytics = LatestBatch.FStatAnalytics ?? "";
        var statusClass = analytics.Equals("pass", StringComparison.OrdinalIgnoreCase) ? STAT_PASS_CLASS : STAT_FAIL_CLASS;

        return $"{fStatValue}/<span class=\"{statusClass}\">{analytics}</span>";
    }

    private string GetSignificanceValue() =>
        _disposed ? NOT_AVAILABLE : (LatestBatch?.B1PValue != null ? Math.Round(LatestBatch.B1PValue, 4).ToString("F4") : NOT_AVAILABLE);

    // Custom label formatter for chart axes (matching original JavaScript formatter)
    private string FormatAxisLabel(double value)
    {
        if (_disposed) return "0";

        var x = value / 1000;
        if (value > 0)
        {
            return $"${x:F0}K";
        }
        else
        {
            return $"$({Math.Abs(x):F0})K";
        }
    }

    /// <summary>
    /// Checks if user has required role for hedge relationship operations.
    /// Legacy: checkUserRole('24') || checkUserRole('17') || checkUserRole('5')
    /// </summary>
    private bool HasRequiredRole()
    {
        return CheckUserRole("24") || CheckUserRole("17") || CheckUserRole("5");
    }

    /// <summary>
    /// Checks if user has a specific role.
    /// Legacy: checkUserRole() function in hr_hedgeRelationshipAddEditCtrl.js
    /// </summary>
    private bool CheckUserRole(string role)
    {
        if (string.IsNullOrEmpty(role) || UserAuthData?.Roles == null)
            return false;

        // Parse string role ID to integer and cast to EdgeRole enum
        if (!int.TryParse(role, out var roleId))
            return false;

        var edgeRole = (DerivativeEDGE.Authorization.AuthClaims.EdgeRole)roleId;
        return UserAuthData.Roles.Contains(edgeRole);
    }

    /// <summary>
    /// Determines if Delete option should be visible in dropdown.
    /// Legacy: data-ng-show="Model.HedgeState === 'Draft' || checkUserRole('24') || checkUserRole('17') || checkUserRole('5')"
    /// </summary>
    private bool CanShowDeleteOption()
    {
        if (HedgeRelationship == null)
            return false;

        return HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft || HasRequiredRole();
    }
    #endregion

    #region Data Models
    public class ChartDataModel
    {
        public double XValue { get; set; }
        public double YValue { get; set; }
    }
    #endregion
}
