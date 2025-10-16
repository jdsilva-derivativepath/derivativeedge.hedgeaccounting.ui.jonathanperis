using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.Common.Extensions;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Enums;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Helpers;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;
using Microsoft.JSInterop;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Navigations;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages;

public partial class HedgeRelationshipDetails
{
    #region Parameters
    [Parameter] public long HedgeId { get; set; }
    #endregion

    #region Injected Services
    [Inject] private IAlertService AlertService { get; set; }
    [Inject] private NavigationManager NavManager { get; set; }
    [Inject] private IMediator Mediator { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IUserAuthData UserAuthData { get; set; }
    #endregion

    #region Constants
    private const string MODAL_AMORTIZATION = "Amortization";
    private const string MODAL_OPTION_AMORTIZATION = "Option Amortization";
    private const string MODAL_HEDGE_DOCUMENT_PREVIEW = "HedgeDocumentPreview";
    private const string MODAL_DEDESIGNATE = "DeDesignate";
    private const string MODAL_REDESIGNATE = "ReDesignate";
    private const string STANDARD_MODAL_WIDTH = "33rem";
    private const string WIDE_MODAL_WIDTH = "41rem";
    private const string NARROW_MODAL_WIDTH = "30rem";
    private bool IsNewHedgeDocumentTemplate => string.IsNullOrWhiteSpace(HedgeRelationship.Objective)
        || IsHtmlWhitespaceOnly(HedgeRelationship.Objective);

    private bool IsHtmlWhitespaceOnly(string html)
    {
        // Regex pattern that matches only "empty" HTML content (tags or spaces)
        var pattern = @"^(?:</?p>|</?br\s*/?>|</?div>|</?span>|\s|&nbsp;)*$";
        return Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase);
    }
    #endregion

    #region Private Properties
    private DateTime? CurveDate { get; set; } = DateTime.Today;
    private DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    private string OpenModal { get; set; } = string.Empty;
    private SfTab hedgerelationshiptabRef;
    private InstrumentAnalysisTab instrumentAnalysisTabRef;
    private TestResultsTab testResultsTabRef;
    private List<string> ValidationErrors { get; set; } = new();
    private DateTime? DesignationDate
    {
        get => !string.IsNullOrEmpty(HedgeRelationship?.DesignationDate)
               ? DateTime.Parse(HedgeRelationship.DesignationDate)
               : null;
        set
        {
            if (HedgeRelationship != null)
                HedgeRelationship.DesignationDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private DateTime? DeDesignationDate
    {
        get => !string.IsNullOrEmpty(HedgeRelationship?.DedesignationDate)
               ? DateTime.Parse(HedgeRelationship.DedesignationDate)
               : null;
        set
        {
            if (HedgeRelationship != null)
                HedgeRelationship.DedesignationDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private DateTime? AmortizationStartDate
    {
        get => !string.IsNullOrEmpty(AmortizationModel?.StartDate)
               ? DateTime.Parse(AmortizationModel.StartDate)
               : null;
        set
        {
            if (AmortizationModel != null)
                AmortizationModel.StartDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private DateTime? AmortizationEndDate
    {
        get => !string.IsNullOrEmpty(AmortizationModel?.EndDate)
               ? DateTime.Parse(AmortizationModel.EndDate)
               : null;
        set
        {
            if (AmortizationModel != null)
                AmortizationModel.EndDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private DateTime? AmortizationFrontRollDate
    {
        get => !string.IsNullOrEmpty(AmortizationModel?.FrontRollDate)
               ? DateTime.Parse(AmortizationModel.FrontRollDate)
               : null;
        set
        {
            if (AmortizationModel != null)
                AmortizationModel.FrontRollDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private DateTime? AmortizationBackRollDate
    {
        get => !string.IsNullOrEmpty(AmortizationModel?.BackRollDate)
               ? DateTime.Parse(AmortizationModel.BackRollDate)
               : null;
        set
        {
            if (AmortizationModel != null)
                AmortizationModel.BackRollDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private List<string> AmortizationFinancialCenters { get; set; }
    private DateTime? OptionAmortizationStartDate
    {
        get => !string.IsNullOrEmpty(OptionAmortizationModel?.StartDate)
               ? DateTime.Parse(OptionAmortizationModel.StartDate)
               : null;
        set
        {
            if (OptionAmortizationModel != null)
                OptionAmortizationModel.StartDate = value?.ToString("MM/dd/yyyy");
        }
    }
    private DateTime? OptionAmortizationEndDate
    {
        get => !string.IsNullOrEmpty(OptionAmortizationModel?.EndDate)
               ? DateTime.Parse(OptionAmortizationModel.EndDate)
               : null;
        set
        {
            if (OptionAmortizationModel != null)
                OptionAmortizationModel.EndDate = value?.ToString("MM/dd/yyyy");
        }
    }
    #endregion

    #region Data Collections
    public List<Client> AvailableClients { get; private set; } = new();
    public List<DerivativeEDGEHAEntityLegalEntity> AvailableEntities { get; private set; } = new();

    public List<DerivativeEDGEHAEntityGLAccount> AmortizationGLAccounts { get; private set; } = new();
    public List<DerivativeEDGEHAEntityGLAccount> AmortizationContraAccounts { get; private set; } = new();

    public List<DerivativeEDGEHAEntityGLAccount> OptionAmortizationGLAccounts { get; private set; } = new();
    public List<DerivativeEDGEHAEntityGLAccount> OptionAmortizationContraAccounts { get; private set; } = new();

    public List<DerivativeEDGEHAEntityGLAccount> IntrinsicAmortizationGLAccounts { get; private set; } = new();
    public List<DerivativeEDGEHAEntityGLAccount> IntrinsicAmortizationContraAccounts { get; private set; } = new();

    // Enum‐based data sources
    public List<FinancialCenterOption> AvailableFinancialCenters { get; private set; }
        = GetFinancialCenterOptions();
    public List<PaymentFrequencyOption> AvailablePaymentFrequencies { get; private set; }
        = GetPaymentFrequencyOptions();
    public List<DayCountConvOption> AvailableDayCountConventions { get; private set; }
        = GetDayCountConvOptions();
    public List<PayBusDayConvOption> AvailablePayBusDayConventions { get; private set; }
        = GetPayBusDayConvOptions();
    #endregion

    #region Loading States
    public bool IsLoadingHedgeRelationship { get; set; }
    public bool IsLoadingClients { get; set; }
    public bool IsLoadingEntities { get; set; }
    public bool IsGeneratingInceptionPackage { get; set; }
    public bool IsDownloadingSpecsAndChecks { get; set; }
    public bool IsSavingHedgeRelationship { get; set; }
    public bool IsRunningRegression { get; set; }
    private bool InProgress = false;

    private bool IsInProgress =>
        IsLoadingHedgeRelationship ||
        IsLoadingClients ||
        IsLoadingEntities ||
        IsGeneratingInceptionPackage ||
        IsDownloadingSpecsAndChecks ||
        IsSavingHedgeRelationship ||
        IsRunningRegression;
    #endregion

    #region UI State Properties
    private bool IsAmortizationModal => OpenModal == MODAL_AMORTIZATION;
    private bool IsOptionAmortizationModal => OpenModal == MODAL_OPTION_AMORTIZATION;
    private bool IsHedgeDocumentPreviewModal => OpenModal == MODAL_HEDGE_DOCUMENT_PREVIEW;
    private bool IsDeDesignateModal => OpenModal == MODAL_DEDESIGNATE;
    private bool IsReDesignateModal => OpenModal == MODAL_REDESIGNATE;
    private string BenchMarkLabel => HedgeRelationshipLabelHelper.GetBenchMarkLabel(HedgeRelationship);

    // TODO: Replace with actual business logic
    private bool IsHedgeDesignated => false; // Replace with real data check

    // Individual checkbox states - these should be bound to specific properties in the model
    private bool QualitativeAssessment { get; set; }
    private bool Acquisition { get; set; }
    private bool AdjustDates { get; set; }
    private bool Straightline { get; set; }
    private bool IncludeInRegression { get; set; }
    private bool AmortizeOptionPremium { get; set; }

    // De-Designate Dialog Properties
    private int DedesignationReason { get; set; } = 0;
    private DateTime? DedesignationDateDialog { get; set; }
    private int CashPaymentType { get; set; } = 0;
    private decimal? DedesignatePayment { get; set; } = 0;
    private decimal? DedesignateAccrual { get; set; } = 0;
    private decimal? BasisAdjustment { get; set; } = 0;
    private decimal? BasisAdjustmentBalance { get; set; } = 0;
    private bool ShowBasisAdjustmentBalance { get; set; }
    private DateTime? DedesignateTimeValuesStartDate { get; set; }
    private DateTime? DedesignateTimeValuesEndDate { get; set; }
    private bool HedgedExposureExist { get; set; } = true;
    private string DedesignateUserMessage { get; set; } = string.Empty;
    private bool DedesignateIsError { get; set; }
    private bool IsDeDesignateDisabled { get; set; } = true;

    // Re-Designate Dialog Properties
    private DateTime? RedesignationDate { get; set; }
    private DateTime? RedesignateTimeValuesStartDate { get; set; }
    private DateTime? RedesignateTimeValuesEndDate { get; set; }
    private decimal? RedesignatePayment { get; set; } = 0;
    private string RedesignatePaymentFrequency { get; set; } = string.Empty;
    private string RedesignateDayCountConv { get; set; } = string.Empty;
    private string RedesignatePayBusDayConv { get; set; } = string.Empty;
    private bool RedesignateAdjustedDates { get; set; }
    private bool MarkAsAcquisition { get; set; }
    private bool IsDocTemplateFound { get; set; }
    #endregion

    #region Models and Data
    public DocumentContent Model { get; set; } = new();
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM AmortizationModel { get; set; } = new();
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM OptionAmortizationModel { get; set; } = new();
    private List<DropDownMenuItem> WorkflowItems = new();

    public class ChartDataModel
    {
        public string Date { get; set; }
        public double R2Value { get; set; }
        public double Slope { get; set; }
    }

    // Dynamic chart data from HedgeRegressionBatches
    private List<ChartDataModel> EffectivenessChartData { get; set; } = new();

    // Static fallback data (kept for reference)
    private static readonly List<ChartDataModel> ChartData = new()
    {
        new() { Date = new DateTime(2023, 3, 1).ToString("yyyy-MM-dd"), R2Value = 0.85, Slope = 1 },
        new() { Date = new DateTime(2023, 4, 1).ToString("yyyy-MM-dd"), R2Value = 0.9, Slope = 1 },
        new() { Date = new DateTime(2023, 5, 1).ToString("yyyy-MM-dd"), R2Value = 0.87, Slope = 1 },
        new() { Date = new DateTime(2023, 6, 1).ToString("yyyy-MM-dd"), R2Value = 0.92, Slope = 1 },
        new() { Date = new DateTime(2023, 7, 1).ToString("yyyy-MM-dd"), R2Value = 0.89, Slope = 1 },
        new() { Date = new DateTime(2023, 8, 1).ToString("yyyy-MM-dd"), R2Value = 0.91, Slope = 1 },
        new() { Date = new DateTime(2023, 9, 1).ToString("yyyy-MM-dd"), R2Value = 0.88, Slope = 1 }
    };

    private List<ToolbarItemModel> BasicTools = new()
    {
        new() { Command = ToolbarCommand.Bold },
        new() { Command = ToolbarCommand.Italic },
        new() { Command = ToolbarCommand.Underline },
        new() { Command = ToolbarCommand.FontName },
        new() { Command = ToolbarCommand.FontSize },
        new() { Command = ToolbarCommand.FontColor },
        new() { Command = ToolbarCommand.BackgroundColor },
        new() { Command = ToolbarCommand.Alignments },
        new() { Command = ToolbarCommand.OrderedList },
        new() { Command = ToolbarCommand.UnorderedList },
        new() { Command = ToolbarCommand.CreateLink },
        new() { Command = ToolbarCommand.Image },
        new() { Command = ToolbarCommand.SourceCode },
        new() { Command = ToolbarCommand.Undo },
        new() { Command = ToolbarCommand.Redo }
    };
    #endregion

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        BuildWorkflowItems();
    }

    protected override async Task OnInitializedAsync()
    {
        // Load hedge relationship first to ensure data is populated
        await GetHedgeRelationship(HedgeId);

        // Only proceed if HedgeRelationship was successfully loaded
        if (HedgeRelationship != null)
        {
            // Load clients and entities in parallel
            await Task.WhenAll(
                LoadClientsAsync(),
                LoadClientEntitiesAsync(HedgeRelationship.ClientID)
            );
        }
        else
        {
            // Just load clients if hedge relationship failed to load
            await LoadClientsAsync();
        }

        // Finally, load GL accounts if HedgeRelationship is available
        await LoadGLAccounts();
    }
    #endregion

    #region Data Loading Methods
    public async Task GetHedgeRelationship(long hedgeId)
    {
        try
        {
            IsLoadingHedgeRelationship = true;
            StateHasChanged();

            var response = await Mediator.Send(new GetHedgeRelationshipById.Query(hedgeId));
            HedgeRelationship = response.Data;

            if (HedgeRelationship != null)
            {
                HedgeRelationship.BenchmarkText = HedgeRelationshipLabelHelper.GetBenchMarkLabel(HedgeRelationship); // Set BenchMarkLabel on initial load
            }

            // Generate effectiveness chart data after loading hedge relationship
            GenerateEffectivenessChartData();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"There was a problem retrieving the Hedge Relationship: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsLoadingHedgeRelationship = false;
        }
    }

    private void GenerateEffectivenessChartData()
    {
        if (HedgeRelationship?.HedgeRegressionBatches?.Any() == true)
        {
            // Filter batches: take first 8, exclude 'User' type unless HedgeState is 'Draft'
            var filteredBatches = HedgeRelationship.HedgeRegressionBatches
                .Take(8)
                .Where(batch =>
                    (batch.HedgeResultType != DerivativeEDGEHAEntityEnumHedgeResultType.User && HedgeRelationship.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Draft) ||
                    HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft)
                .ToList();

            // Sort by ValueDate
            filteredBatches = filteredBatches
                .OrderBy(batch => batch.ValueDate)
                .ToList();

            var chartData = new List<ChartDataModel>();
            var processedDates = new HashSet<string>();

            foreach (var batch in filteredBatches)
            {
                // Remove duplicates by ValueDate (same logic as original JS)
                if (!processedDates.Contains(batch.ValueDate))
                {
                    // Round to 2 decimal places (same as original JS: Math.round(value + 'e2') + 'e-2')
                    var slope = Math.Round(batch.Slope, 2);
                    var rSquared = Math.Round(batch.RSquared, 2);

                    chartData.Add(new ChartDataModel
                    {
                        Date = batch.ValueDate,
                        R2Value = rSquared,
                        Slope = slope
                    });

                    processedDates.Add(batch.ValueDate);
                }
            }

            EffectivenessChartData = chartData;
        }
        else
        {
            // Fallback to static data converted to DateTime format
            EffectivenessChartData = new List<ChartDataModel> { };
        }
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            IsLoadingClients = true;
            StateHasChanged();

            var query = new GetClients.Query();
            var response = await Mediator.Send(query, CancellationToken.None);
            response.Clients.Insert(0, new Client { ClientId = 0, ClientName = "None" });
            AvailableClients = response.Clients;

            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"There was a problem retrieving the Client List: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsLoadingClients = false;
        }
    }

    private async Task LoadClientEntitiesAsync(long? clientId)
    {
        try
        {
            IsLoadingEntities = true;
            StateHasChanged();

            if (clientId == null || clientId == 0)
            {
                AvailableEntities.Clear();
                if (HedgeRelationship != null)
                {
                    HedgeRelationship.BankEntityID = 0;
                }
                return;  // Prevent loading entities if null or "None" is selected
            }

            // Store current BankEntityID to restore if it exists in new list
            var currentBankEntityID = HedgeRelationship.BankEntityID;

            var query = new GetClientEntities.Query(clientId);
            var response = await Mediator.Send(query, CancellationToken.None);
            response.Entities.Insert(0, new Entity { EntityId = 0, EntityLongName = "None" });
            AvailableEntities = response.Entities.Select(data => new DerivativeEDGEHAEntityLegalEntity
            {
                Id = data.EntityId,
                Name = data.EntityLongName,
            }).ToList();

            // Restore BankEntityID if it exists in the new entities list
            if (HedgeRelationship != null)
            {
                if (currentBankEntityID != 0 && AvailableEntities.Any(e => e.Id == currentBankEntityID))
                {
                    HedgeRelationship.BankEntityID = currentBankEntityID;
                }
                else
                {
                    HedgeRelationship.BankEntityID = 0;
                }
            }
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"There was a problem retrieving the AvailableEntities List: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsLoadingEntities = false;
        }
    }

    private async Task LoadGLAccounts()
    {
        var query = new GetGLAccountsForHedging.Query(HedgeRelationship.ClientID, HedgeRelationship.BankEntityID);
        var result = await Mediator.Send(query);

        AmortizationGLAccounts = result.Data;
        AmortizationContraAccounts = result.Data;

        OptionAmortizationGLAccounts = result.Data;
        OptionAmortizationContraAccounts = result.Data;

        IntrinsicAmortizationGLAccounts = result.Data;
        IntrinsicAmortizationContraAccounts = result.Data;
    }
    #endregion

    #region Event Handlers
    private void BuildWorkflowItems()
    {
        WorkflowItems.Clear();

        var state = HedgeRelationship?.HedgeState;
        var type = HedgeRelationship?.HedgeType;

        // Check user roles - workflow actions are disabled if user doesn't have required roles (24, 17, or 5)
        var hasWorkflowPermission = HasRequiredRole();

        // Legacy logic from setWorkFlow():
        // if ($scope.Model.HedgeState === 'Draft') -> Don't show De-Designate
        // if ($scope.Model.HedgeState !== 'Designated' || $scope.Model.HedgeType !== "CashFlow") -> Don't show Re-Designate
        // if ($scope.Model.HedgeState === 'Designated' || $scope.Model.HedgeState === "Dedesignated") -> Show Redraft instead of Designate

        if (state == DerivativeEDGEHAEntityEnumHedgeState.Draft)
        {
            // Draft state: Show only Designate
            WorkflowItems.Add(new DropDownMenuItem { Text = "Designate", Disabled = !hasWorkflowPermission });
        }
        else if (state == DerivativeEDGEHAEntityEnumHedgeState.Designated)
        {
            // Designated state: Show Redraft, De-Designate, and optionally Re-Designate
            WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
            WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasWorkflowPermission });
            
            // Re-Designate only for CashFlow hedge types (DE-3928)
            if (type == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
            {
                WorkflowItems.Add(new DropDownMenuItem { Text = "Re-Designate", Disabled = !hasWorkflowPermission });
            }
        }
        else if (state == DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
        {
            // Dedesignated state: Show only Redraft (DE-2731)
            WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
        }
    }

    private async Task HandleClientValueChangeAsync()
    {
        if (HedgeRelationship != null)
        {
            await LoadClientEntitiesAsync(HedgeRelationship.ClientID);
        }
    }

    private void HandleEntityValueChangeAsync(ChangeEventArgs<long, DerivativeEDGEHAEntityLegalEntity> args)
    {
        if (HedgeRelationship != null)
        {
            HedgeRelationship.BankEntityID = args.Value;
            StateHasChanged();
        }
    }

    private void NewMenuOnItemSelected(MenuEventArgs args)
    {
        OpenModal = args.Item.Text;
    }

    private void NewMenuDlgButtonOnClose()
    {
        OpenModal = string.Empty; // This closes any open modal
    }

    public async void OnSelectedTab(SelectEventArgs args)
    {
        // Refresh grid data when switching to the "Instruments and Analysis" tab
        if (args.SelectedIndex == 0 && instrumentAnalysisTabRef != null)
        {
            await instrumentAnalysisTabRef.RefreshGridData();
        }
    }

    private Task RedirectToHedgeDocumentService(string pathAndQuery)
    {
        NavManager.NavigateTo(pathAndQuery, forceLoad: true);
        return Task.CompletedTask;
    }

    private Task PreviewHedgeDocumentObjective()
    {
        if (HedgeRelationship == null)
        {
            AlertService.ShowToast("No hedge relationship data available", AlertKind.Warning, "Warning", showButton: true);
            return Task.CompletedTask;
        }

        // Open the hedge document preview dialog
        OpenModal = MODAL_HEDGE_DOCUMENT_PREVIEW;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task GenerateInceptionPackageAsync()
    {
        if (HedgeRelationship == null)
        {
            await AlertService.ShowToast("No hedge relationship data available", AlertKind.Warning, "Warning", showButton: true);
            return;
        }

        try
        {
            IsGeneratingInceptionPackage = true;
            StateHasChanged();

            var query = new InceptionPackageService.Query(HedgeRelationship);
            var result = await Mediator.Send(query);

            // Convert stream to byte array for download
            using var memoryStream = new MemoryStream();
            await result.ExcelStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Trigger file download
            await JSRuntime.InvokeVoidAsync("downloadFile", result.FileName, Convert.ToBase64String(fileBytes));

            await AlertService.ShowToast("Inception package generated successfully!", AlertKind.Success, "Success", showButton: true);
        }
        catch (ArgumentNullException)
        {
            await AlertService.ShowToast("Hedge relationship data is required", AlertKind.Error, "Error", showButton: true);
        }
        catch (UnauthorizedAccessException)
        {
            await AlertService.ShowToast("Authentication failed. Please log in again.", AlertKind.Error, "Authentication Error", showButton: true);
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Failed to generate inception package: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsGeneratingInceptionPackage = false;
            StateHasChanged();
        }
    }

    private async Task DownloadSpecsAndChecksAsync()
    {
        if (HedgeRelationship == null)
        {
            await AlertService.ShowToast("No hedge relationship data available", AlertKind.Warning, "Warning", showButton: true);
            return;
        }

        try
        {
            IsDownloadingSpecsAndChecks = true;
            StateHasChanged();

            var query = new DownloadSpecsAndChecksService.Query(HedgeRelationship);
            var result = await Mediator.Send(query);

            // Convert stream to byte array for download
            using var memoryStream = new MemoryStream();
            await result.ExcelStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Trigger file download
            await JSRuntime.InvokeVoidAsync("downloadFile", result.FileName, Convert.ToBase64String(fileBytes));

            await AlertService.ShowToast("Specs and checks downloaded successfully!", AlertKind.Success, "Success", showButton: true);
        }
        catch (ArgumentNullException)
        {
            await AlertService.ShowToast("Hedge relationship data is required", AlertKind.Error, "Error", showButton: true);
        }
        catch (UnauthorizedAccessException)
        {
            await AlertService.ShowToast("Authentication failed. Please log in again.", AlertKind.Error, "Authentication Error", showButton: true);
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Failed to download specs and checks: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsDownloadingSpecsAndChecks = false;
            StateHasChanged();
        }
    }

    public void CancelButtonHandler()
    {
        NavManager.NavigateTo("/hedgeaccountingapp/hedgerelationship", forceLoad: true);
    }

    private async Task SaveHedgeRelationshipAsync()
    {
        ValidationErrors.Clear();
        if (HedgeRelationship == null)
        {
            await AlertService.ShowToast("No hedge relationship data available", AlertKind.Warning, "Warning", showButton: true);
            return;
        }

        try
        {
            IsSavingHedgeRelationship = true;
            StateHasChanged();

            var result = await Mediator.Send(new UpdateHedgeRelationship.Command(HedgeRelationship));
            if (!result.HasError)
            {
                // Reload from backend to ensure all fields are correct
                await GetHedgeRelationship(HedgeId);
                await AlertService.ShowToast("Hedge relationship updated successfully!", AlertKind.Success, "Success", showButton: true);
            }
            else
            {
                await AlertService.ShowToast(result.Message, AlertKind.Error, "Failed", showButton: true);
            }
        }
        catch (ArgumentNullException)
        {
            await AlertService.ShowToast("Hedge relationship data is required", AlertKind.Error, "Error", showButton: true);
        }
        catch (UnauthorizedAccessException)
        {
            await AlertService.ShowToast("Authentication failed. Please log in again.", AlertKind.Error, "Authentication Error", showButton: true);
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Failed to save hedge relationship: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsSavingHedgeRelationship = false;
            StateHasChanged();
        }
    }

    private async Task RunRegressionAsync()
    {
        if (HedgeRelationship == null)
        {
            await AlertService.ShowToast("No hedge relationship data available", AlertKind.Warning, "Warning", showButton: true);
            return;
        }

        ValidationErrors = RegressionRequirementsValidator.Validate(HedgeRelationship);
        if (ValidationErrors.Any())
        {
            StateHasChanged();
            return;
        }

        try
        {
            IsRunningRegression = true;
            StateHasChanged();

            // Check analytics status first (mimicking JavaScript checkAnalyticsStatus)
            var analyticsStatusQuery = new CheckAnalyticsStatus.Query();
            var analyticsResponse = await Mediator.Send(analyticsStatusQuery);

            if (analyticsResponse.HasError)
            {
                await AlertService.ShowToast("Failed to check analytics service status", AlertKind.Error, "Error", showButton: true);
                return;
            }

            if (!analyticsResponse.IsAnalyticsAvailable)
            {
                // Show confirmation dialog similar to JavaScript confirm
                var proceed = await JSRuntime.InvokeAsync<bool>("confirm",
                    "Analytics service is currently unavailable. Are you sure you want to continue?");

                if (!proceed)
                {
                    return;
                }
            }

            // Run the regression analysis
            var regressionCommand = new RunRegression.Command(HedgeRelationship);
            var regressionResponse = await Mediator.Send(regressionCommand);

            if (regressionResponse.HasError)
            {
                if (regressionResponse.ValidationErrors?.Any() == true)
                {
                    var errorMessage = string.Join("; ", regressionResponse.ValidationErrors);
                    await AlertService.ShowToast($"Regression failed: {errorMessage}", AlertKind.Error, "Regression Error", showButton: true);
                }
                else
                {
                    await AlertService.ShowToast(regressionResponse.ErrorMessage ?? "Failed to run regression analysis", AlertKind.Error, "Error", showButton: true);
                }
                return;
            }

            // Update the hedge relationship with the new data (mimicking refreshAfterRegress)
            if (regressionResponse.Data != null)
            {
                HedgeRelationship = regressionResponse.Data;

                // Regenerate effectiveness chart data in main component
                GenerateEffectivenessChartData();

                // Refresh the Instruments and Analysis tab if it's loaded
                if (instrumentAnalysisTabRef != null)
                {
                    await instrumentAnalysisTabRef.RefreshGridData();
                }

                // *** KEY FIX: Refresh the TestResultsTab with new data ***
                if (testResultsTabRef != null)
                {
                    await testResultsTabRef.RefreshTestResultsData();
                }

                await AlertService.ShowToast("Regression analysis completed successfully!", AlertKind.Success, "Success", showButton: true);

                // Switch to the "Test Results" tab (Tab index 1 - Test Results)
                if (hedgerelationshiptabRef != null)
                {
                    await hedgerelationshiptabRef.SelectAsync(1);
                }
            }
            else
            {
                // If no data returned, refresh the hedge relationship from the API
                await GetHedgeRelationship(HedgeId);

                // *** Refresh the TestResultsTab after API refresh ***
                if (testResultsTabRef != null)
                {
                    await testResultsTabRef.RefreshTestResultsData();
                }

                await AlertService.ShowToast("Regression analysis completed successfully!", AlertKind.Success, "Success", showButton: true);

                // Switch to the "Test Results" tab
                if (hedgerelationshiptabRef != null)
                {
                    await hedgerelationshiptabRef.SelectAsync(1);
                }
            }
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Failed to run regression analysis: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsRunningRegression = false;
            StateHasChanged();
        }
    }

    private bool CheckUserRole(string role)
    {
        if (string.IsNullOrEmpty(role) || UserAuthData?.Roles == null)
            return false;

        return UserAuthData.Roles.Any(userRole => userRole.ToString() == role);
    }

    private bool HasRequiredRole() =>
        CheckUserRole("24") || CheckUserRole("17") || CheckUserRole("5");

    private bool IsSaveDisabled() =>
        HedgeRelationship == null ||
        IsInProgress ||
        (!HasRequiredRole() && HedgeRelationship.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Draft);

    private bool IsPreviewInceptionPackageDisabled()
    {
        if (HedgeRelationship == null)
            return true;

        return IsInProgress ||
               HedgeRelationship.LatestHedgeRegressionBatch == null ||
               (!HasRequiredRole() &&
                HedgeRelationship.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Draft);
    }

    private bool IsRegressionDisabled()
    {
        if (HedgeRelationship == null)
            return true;

        var benchmarkInvalid = (HedgeRelationship.Benchmark == DerivativeEDGEHAEntityEnumBenchmark.None)
                              && (HedgeRelationship.HedgeRiskType != DerivativeEDGEHAEntityEnumHedgeRiskType.ForeignExchange);

        return IsInProgress ||
               benchmarkInvalid ||
               (!HasRequiredRole() &&
                HedgeRelationship.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Draft);
    }

    private bool IsBackloadDisabled()
    {
        if (HedgeRelationship == null)
            return true;

        return IsInProgress ||
               !HasRequiredRole() ||
               HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft;
    }

    private async Task BackloadAsync()
    {
        if (HedgeRelationship == null)
        {
            await AlertService.ShowToast("No hedge relationship data available", AlertKind.Warning, "Warning", showButton: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(HedgeRelationship.HedgeDocumentTemplateName) && (!IsNewHedgeDocumentTemplate || HedgeRelationship.IsCaarHedgeTemplate))
        {
            // If needed, perform any rich text normalization here similar to legacy setDetailFormatted
            // For now we leave content as-is; add placeholder comment for future formatting implementation.
            // TODO: Apply formatting normalization similar to legacy setDetailFormatted for Objective and HedgedItemTypeDesc.
        }

        var existsBackload = HedgeRelationship.HedgeRegressionBatches?.Any(b => b.HedgeResultType == DerivativeEDGEHAEntityEnumHedgeResultType.Backload) == true;
        if (existsBackload)
        {
            await AlertService.ShowToast("Backload already created.", AlertKind.Error, "Backload", showButton: true);
            return;
        }

        try
        {
            var response = await Mediator.Send(new RunRegression.Command(HedgeRelationship, DerivativeEDGEHAEntityEnumHedgeResultType.Backload));
            if (!response.HasError) { HedgeRelationship = response.Data; GenerateEffectivenessChartData(); }

            await AlertService.ShowToast("Backload requested (API call pending implementation).", AlertKind.Success, "Success", showButton: true);
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Failed to create Backload: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task HandleWorkflowAction(MenuEventArgs args)
    {
        var selected = args.Item.Text;

        switch (selected)
        {
            case "Designate":
                await HandleDesignateAsync();
                break;
            case "De-Designate":
                await HandleDeDesignateAsync();
                break;
            case "Redraft":
                await HandleRedraftAsync();
                break;
            case "Re-Designate":
                await HandleReDesignateAsync();
                break;
        }
    }

    private async Task HandleDesignateAsync()
    {
        // Validate designation requirements
        ValidationErrors = DesignationRequirementsValidator.Validate(HedgeRelationship);
        if (ValidationErrors.Any())
        {
            StateHasChanged();
            return;
        }

        try
        {
            // Save current state before designation
            await SaveHedgeRelationshipAsync();

            // Execute designation workflow
            var response = await Mediator.Send(new DesignateHedgeRelationship.Command(HedgeId));
            
            if (response.HasError)
            {
                await AlertService.ShowToast(response.ErrorMessage, AlertKind.Error, "Designation Failed", showButton: true);
                return;
            }

            // Update the local hedge relationship with the latest state
            HedgeRelationship = response.HedgeRelationship;
            
            await AlertService.ShowToast("Hedge Relationship successfully designated.", AlertKind.Success, "Success", showButton: true);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error during designation: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task HandleDeDesignateAsync()
    {
        try
        {
            // Initialize de-designation model with default values
            // Following the legacy logic from onChangeActionValue "De-Designate"
            
            DedesignateUserMessage = string.Empty;
            DedesignateIsError = false;
            IsDeDesignateDisabled = true;
            DedesignationReason = 0;
            DedesignateTimeValuesStartDate = DateTime.Today;
            DedesignateTimeValuesEndDate = DateTime.Today;
            CashPaymentType = 0;
            HedgedExposureExist = true;
            DedesignationDateDialog = DateTime.Today;
            DedesignatePayment = 0;
            DedesignateAccrual = 0;
            BasisAdjustment = 0;
            BasisAdjustmentBalance = 0;

            // API Call: Get termination date from hedging item
            // if (HedgeRelationship.HedgingItems?.Any() == true)
            // {
            //     var lastHedgingItem = HedgeRelationship.HedgingItems.Last();
            //     
            //     // API Call: Get termination date
            //     var terminationDateResponse = await Mediator.Send(
            //         new GetTerminationDate.Query(lastHedgingItem.ItemID));
            //     
            //     if (terminationDateResponse.TerminationDate != null)
            //     {
            //         // API Call: Price the instrument to get accrual
            //         var pricingResponse = await Mediator.Send(
            //             new PriceInstrument.Query(
            //                 lastHedgingItem.ItemID, 
            //                 terminationDateResponse.TerminationDate, 
            //                 lastHedgingItem.SecurityType));
            //         
            //         // Set accrual from pricing response
            //         DedesignateAccrual = pricingResponse.Accrual;
            //     }
            // }

            // Show De-Designation dialog
            OpenModal = MODAL_DEDESIGNATE;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error during de-designation: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task OnDeDesignateReasonChanged(int reason)
    {
        DedesignationReason = reason;
        
        try
        {
            // API Call: Load de-designation data for the selected reason
            var response = await Mediator.Send(
                new GetDeDesignateData.Query(HedgeId, (DerivativeEDGEHAEntityEnumDedesignationReason)reason));
            
            if (response.HasError || !string.IsNullOrEmpty(response.ErrorMessage))
            {
                IsDeDesignateDisabled = true;
                DedesignateUserMessage = response.ErrorMessage ?? "An error occurred loading de-designation data";
                DedesignateIsError = true;
                DedesignationDateDialog = response.DedesignationDate;
            }
            else
            {
                IsDeDesignateDisabled = false;
                DedesignateUserMessage = string.Empty;
                DedesignateTimeValuesStartDate = response.TimeValuesStartDate;
                DedesignateTimeValuesEndDate = response.TimeValuesEndDate;
                DedesignationDateDialog = response.DedesignationDate;
                DedesignatePayment = response.Payment;
                ShowBasisAdjustmentBalance = response.ShowBasisAdjustmentBalance;
                BasisAdjustment = response.BasisAdjustment;
                BasisAdjustmentBalance = response.BasisAdjustmentBalance;
                CashPaymentType = 0;
                HedgedExposureExist = true;
            }
        }
        catch (Exception ex)
        {
            IsDeDesignateDisabled = true;
            DedesignateUserMessage = $"Error loading de-designation data: {ex.Message}";
            DedesignateIsError = true;
        }
        
        StateHasChanged();
    }

    private async Task OnDeDesignateConfirmed()
    {
        try
        {
            // Execute de-designation
            var command = new DeDesignateHedgeRelationship.Command(
                HedgeRelationshipId: HedgeId,
                DedesignationDate: DedesignationDateDialog,
                DedesignationReason: DedesignationReason,
                Payment: DedesignatePayment,
                TimeValuesStartDate: DedesignateTimeValuesStartDate,
                TimeValuesEndDate: DedesignateTimeValuesEndDate,
                CashPaymentType: CashPaymentType,
                HedgedExposureExist: HedgedExposureExist,
                BasisAdjustment: BasisAdjustment,
                BasisAdjustmentBalance: BasisAdjustmentBalance);
            
            var response = await Mediator.Send(command);

            if (response.HasError)
            {
                await AlertService.ShowToast(response.ErrorMessage, AlertKind.Error, "De-designation Failed", showButton: true);
                return;
            }

            // Update local hedge relationship
            HedgeRelationship = response.Data;
            
            // Close the modal
            OpenModal = string.Empty;
            
            await AlertService.ShowToast("Hedge Relationship de-designated successfully.", AlertKind.Success, "Success", showButton: true);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error during de-designation: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task HandleRedraftAsync()
    {
        try
        {
            // Execute redraft
            var response = await Mediator.Send(new RedraftHedgeRelationship.Command(HedgeId));
            
            if (response.HasError)
            {
                await AlertService.ShowToast(response.ErrorMessage, AlertKind.Error, "Redraft Failed", showButton: true);
                return;
            }

            // Update local hedge relationship
            HedgeRelationship = response.Data;
            
            await AlertService.ShowToast("Hedge Relationship successfully redrafted.", AlertKind.Success, "Success", showButton: true);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error during redraft: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task HandleReDesignateAsync()
    {
        try
        {
            // Check if document template exists
            var findDocTemplateResponse = await Mediator.Send(new FindDocumentTemplate.Query(HedgeId));
            
            if (findDocTemplateResponse.HasError)
            {
                await AlertService.ShowToast(findDocTemplateResponse.ErrorMessage, AlertKind.Error, "Re-designation Failed", showButton: true);
                return;
            }

            IsDocTemplateFound = findDocTemplateResponse.HasTemplate;
            
            if (IsDocTemplateFound)
            {
                // Save current state first
                await SaveHedgeRelationshipAsync();
                
                // Reload hedge relationship
                await GetHedgeRelationship(HedgeId);
            }

            // Get re-designation data
            var redesignateResponse = await Mediator.Send(new GetReDesignateData.Query(HedgeId));
            
            if (redesignateResponse.HasError)
            {
                await AlertService.ShowToast(redesignateResponse.ErrorMessage, AlertKind.Error, "Re-designation Failed", showButton: true);
                return;
            }

            // Set model properties from response
            RedesignationDate = redesignateResponse.RedesignationDate;
            RedesignateTimeValuesStartDate = redesignateResponse.TimeValuesStartDate;
            RedesignateTimeValuesEndDate = redesignateResponse.TimeValuesEndDate;
            RedesignatePayment = redesignateResponse.Payment;
            RedesignateDayCountConv = redesignateResponse.DayCountConv;
            RedesignatePayBusDayConv = redesignateResponse.PayBusDayConv;
            RedesignatePaymentFrequency = redesignateResponse.PaymentFrequency;
            RedesignateAdjustedDates = redesignateResponse.AdjustedDates;
            MarkAsAcquisition = redesignateResponse.MarkAsAcquisition;

            // Show Re-Designation dialog
            OpenModal = MODAL_REDESIGNATE;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error during re-designation: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task OnReDesignateConfirmed()
    {
        try
        {
            // Execute re-designation
            var command = new ReDesignateHedgeRelationship.Command(
                HedgeRelationshipId: HedgeId,
                RedesignationDate: RedesignationDate,
                Payment: RedesignatePayment,
                TimeValuesStartDate: RedesignateTimeValuesStartDate,
                TimeValuesEndDate: RedesignateTimeValuesEndDate,
                PaymentFrequency: RedesignatePaymentFrequency,
                DayCountConv: RedesignateDayCountConv,
                PayBusDayConv: RedesignatePayBusDayConv,
                AdjustedDates: RedesignateAdjustedDates,
                MarkAsAcquisition: MarkAsAcquisition);
            
            var response = await Mediator.Send(command);

            if (response.HasError)
            {
                await AlertService.ShowToast(response.ErrorMessage, AlertKind.Error, "Re-designation Failed", showButton: true);
                return;
            }

            // Update local hedge relationship
            HedgeRelationship = response.Data;
            
            // Close the modal
            OpenModal = string.Empty;
            
            await AlertService.ShowToast("Hedge Relationship re-designated successfully.", AlertKind.Success, "Success", showButton: true);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error during re-designation: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }
    #endregion

    #region Form Submission Methods
    private async Task OnSubmitAmortization(EditContext context)
    {
        try
        {
            AmortizationModel.HedgeRelationshipID = HedgeRelationship.ID;
            AmortizationModel.OptionTimeValueAmortType = DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.Amortization;

            if (AmortizationFinancialCenters != null)
            {
                AmortizationModel.FinancialCenters = AmortizationFinancialCenters
                    .Select(s => Enum.TryParse<DerivativeEDGEDomainEntitiesEnumsFinancialCenter>(s, out var result) ? result : default)
                    .Where(fc => fc != default)
                    .ToList();
            }

            var isUpdate = AmortizationModel.ID > 0;
            var successMessage = isUpdate ? "Success! Amortization Updated." : "Success! Amortization Created.";

            var response = await Mediator.Send(new CreateHedgeRelationshipOptionTimeValueAmort.Command(AmortizationModel, HedgeRelationship));

            await AlertService.ShowToast(successMessage, AlertKind.Success, "Success", showButton: true);

            OpenModal = string.Empty;
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error saving Amortization: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private async Task OnSubmitOptionAmortization(EditContext context)
    {
        try
        {
            AmortizationModel.HedgeRelationshipID = HedgeRelationship.ID;
            AmortizationModel.OptionTimeValueAmortType = DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.OptionTimeValue;

            var isUpdate = AmortizationModel.ID > 0;
            var successMessage = isUpdate ? "Success! Option Amortization Updated." : "Success! Option Amortization Created.";

            var response = await Mediator.Send(new CreateHedgeRelationshipOptionTimeValueAmort.Command(AmortizationModel, HedgeRelationship));

            await AlertService.ShowToast(successMessage, AlertKind.Success, "Success", showButton: true);

            OpenModal = string.Empty;
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error saving Option Amortization: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }
    #endregion

    #region Helper Methods
    private void OnAmortizationComboBoxCreated(object args)
    {
        // Set the first item as the default when available
        if (AmortizationGLAccounts?.Any() == true && AmortizationModel.GLAccountID == 0)
        {
            AmortizationModel.GLAccountID = AmortizationGLAccounts.First().Id;
            StateHasChanged();
        }

        // Set the first item as the default when available
        if (AmortizationContraAccounts?.Any() == true && AmortizationModel.ContraAccountID == 0)
        {
            AmortizationModel.ContraAccountID = AmortizationContraAccounts.First().Id;
            StateHasChanged();
        }
    }

    private void OnOptionAmortizationComboBoxCreated(object args)
    {
        // Set the first item as the default when available
        if (OptionAmortizationGLAccounts?.Any() == true && OptionAmortizationModel.GLAccountID == 0)
        {
            OptionAmortizationModel.GLAccountID = OptionAmortizationGLAccounts.First().Id;
            StateHasChanged();
        }

        // Set the first item as the default when available
        if (OptionAmortizationContraAccounts?.Any() == true && OptionAmortizationModel.ContraAccountID == 0)
        {
            OptionAmortizationModel.ContraAccountID = OptionAmortizationContraAccounts.First().Id;
            StateHasChanged();
        }
    }

    public static List<HedgingInstrumentStructureOption> GetHedgingInstrumentStructureOptions() => new()
    {
        new() { Value = HedgingInstrumentStructure.SingleInstrument, Text = HedgingInstrumentStructure.SingleInstrument.GetDescription() },
        new() { Value = HedgingInstrumentStructure.StructuredProduct, Text = HedgingInstrumentStructure.StructuredProduct.GetDescription() },
        new() { Value = HedgingInstrumentStructure.MultipleInstruments, Text = HedgingInstrumentStructure.MultipleInstruments.GetDescription() }
    };

    public class HedgingInstrumentStructureOption
    {
        public HedgingInstrumentStructure Value { get; set; }
        public string Text { get; set; }
    }

    public static List<FinancialCenterOption> GetFinancialCenterOptions() => new()
    {
        new() { Value = FinancialCenter.BEBR, Text = FinancialCenter.BEBR.GetDescription() },
        new() { Value = FinancialCenter.ARBA, Text = FinancialCenter.ARBA.GetDescription() },
        new() { Value = FinancialCenter.ATVI, Text = FinancialCenter.ATVI.GetDescription() },
        new() { Value = FinancialCenter.AUME, Text = FinancialCenter.AUME.GetDescription() },
        new() { Value = FinancialCenter.AUSY, Text = FinancialCenter.AUSY.GetDescription() },
        new() { Value = FinancialCenter.BRSP, Text = FinancialCenter.BRSP.GetDescription() },
        new() { Value = FinancialCenter.CAMO, Text = FinancialCenter.CAMO.GetDescription() },
        new() { Value = FinancialCenter.CATO, Text = FinancialCenter.CATO.GetDescription() },
        new() { Value = FinancialCenter.CHGE, Text = FinancialCenter.CHGE.GetDescription() },
        new() { Value = FinancialCenter.SKBR, Text = FinancialCenter.SKBR.GetDescription() },
        new() { Value = FinancialCenter.CLSA, Text = FinancialCenter.CLSA.GetDescription() },
        new() { Value = FinancialCenter.CNBE, Text = FinancialCenter.CNBE.GetDescription() },
        new() { Value = FinancialCenter.CZPR, Text = FinancialCenter.CZPR.GetDescription() },
        new() { Value = FinancialCenter.DEFR, Text = FinancialCenter.DEFR.GetDescription() },
        new() { Value = FinancialCenter.DKCO, Text = FinancialCenter.DKCO.GetDescription() },
        new() { Value = FinancialCenter.EETA, Text = FinancialCenter.EETA.GetDescription() },
        new() { Value = FinancialCenter.ESMA, Text = FinancialCenter.ESMA.GetDescription() },
        new() { Value = FinancialCenter.FIHE, Text = FinancialCenter.FIHE.GetDescription() },
        new() { Value = FinancialCenter.FRPA, Text = FinancialCenter.FRPA.GetDescription() },
        new() { Value = FinancialCenter.GBLO, Text = FinancialCenter.GBLO.GetDescription() },
        new() { Value = FinancialCenter.GRAT, Text = FinancialCenter.GRAT.GetDescription() },
        new() { Value = FinancialCenter.HKHK, Text = FinancialCenter.HKHK.GetDescription() },
        new() { Value = FinancialCenter.HUBU, Text = FinancialCenter.HUBU.GetDescription() },
        new() { Value = FinancialCenter.IDJA, Text = FinancialCenter.IDJA.GetDescription() },
        new() { Value = FinancialCenter.IEDU, Text = FinancialCenter.IEDU.GetDescription() },
        new() { Value = FinancialCenter.ILTA, Text = FinancialCenter.ILTA.GetDescription() },
        new() { Value = FinancialCenter.ITMI, Text = FinancialCenter.ITMI.GetDescription() },
        new() { Value = FinancialCenter.ITRO, Text = FinancialCenter.ITRO.GetDescription() },
        new() { Value = FinancialCenter.JPTO, Text = FinancialCenter.JPTO.GetDescription() },
        new() { Value = FinancialCenter.KRSE, Text = FinancialCenter.KRSE.GetDescription() },
        new() { Value = FinancialCenter.LBBE, Text = FinancialCenter.LBBE.GetDescription() },
        new() { Value = FinancialCenter.LULU, Text = FinancialCenter.LULU.GetDescription() },
        new() { Value = FinancialCenter.MYKL, Text = FinancialCenter.MYKL.GetDescription() },
        new() { Value = FinancialCenter.MXMC, Text = FinancialCenter.MXMC.GetDescription() },
        new() { Value = FinancialCenter.NLAM, Text = FinancialCenter.NLAM.GetDescription() },
        new() { Value = FinancialCenter.NOOS, Text = FinancialCenter.NOOS.GetDescription() },
        new() { Value = FinancialCenter.NYFD, Text = FinancialCenter.NYFD.GetDescription() },
        new() { Value = FinancialCenter.NYSE, Text = FinancialCenter.NYSE.GetDescription() },
        new() { Value = FinancialCenter.NZAU, Text = FinancialCenter.NZAU.GetDescription() },
        new() { Value = FinancialCenter.NZWE, Text = FinancialCenter.NZWE.GetDescription() },
        new() { Value = FinancialCenter.PAPC, Text = FinancialCenter.PAPC.GetDescription() },
        new() { Value = FinancialCenter.PHMA, Text = FinancialCenter.PHMA.GetDescription() },
        new() { Value = FinancialCenter.PLWA, Text = FinancialCenter.PLWA.GetDescription() },
        new() { Value = FinancialCenter.PTLI, Text = FinancialCenter.PTLI.GetDescription() },
        new() { Value = FinancialCenter.RUMO, Text = FinancialCenter.RUMO.GetDescription() },
        new() { Value = FinancialCenter.SARI, Text = FinancialCenter.SARI.GetDescription() },
        new() { Value = FinancialCenter.SEST, Text = FinancialCenter.SEST.GetDescription() },
        new() { Value = FinancialCenter.SGSI, Text = FinancialCenter.SGSI.GetDescription() },
        new() { Value = FinancialCenter.THBA, Text = FinancialCenter.THBA.GetDescription() },
        new() { Value = FinancialCenter.TWTA, Text = FinancialCenter.TWTA.GetDescription() },
        new() { Value = FinancialCenter.TRAN, Text = FinancialCenter.TRAN.GetDescription() },
        new() { Value = FinancialCenter.USCH, Text = FinancialCenter.USCH.GetDescription() },
        new() { Value = FinancialCenter.USLA, Text = FinancialCenter.USLA.GetDescription() },
        new() { Value = FinancialCenter.USGS, Text = FinancialCenter.USGS.GetDescription() },
        new() { Value = FinancialCenter.USNY, Text = FinancialCenter.USNY.GetDescription() },
        new() { Value = FinancialCenter.ZAJO, Text = FinancialCenter.ZAJO.GetDescription() },
        new() { Value = FinancialCenter.CHZU, Text = FinancialCenter.CHZU.GetDescription() },
        new() { Value = FinancialCenter.EUTA, Text = FinancialCenter.EUTA.GetDescription() },
        new() { Value = FinancialCenter.INMU, Text = FinancialCenter.INMU.GetDescription() },
        new() { Value = FinancialCenter.PKKA, Text = FinancialCenter.PKKA.GetDescription() },
        new() { Value = FinancialCenter.RWKI, Text = FinancialCenter.RWKI.GetDescription() },
        new() { Value = FinancialCenter.COBG, Text = FinancialCenter.COBG.GetDescription() },
        new() { Value = FinancialCenter.VNHA, Text = FinancialCenter.VNHA.GetDescription() },
        new() { Value = FinancialCenter.CME, Text = FinancialCenter.CME.GetDescription()  },
        new() { Value = FinancialCenter.GBEDI, Text = FinancialCenter.GBEDI.GetDescription() },
        new() { Value = FinancialCenter.KYGEC, Text = FinancialCenter.KYGEC.GetDescription() },
        new() { Value = FinancialCenter.PELI, Text = FinancialCenter.PELI.GetDescription() }
    };

    public class FinancialCenterOption
    {
        public FinancialCenter Value { get; set; }
        public string Text { get; set; }
    }

    private static List<PaymentFrequencyOption> GetPaymentFrequencyOptions() => new()
    {
        new() { Value = PaymentFrequency.Monthly,     Text = PaymentFrequency.Monthly.GetDescription() },
        new() { Value = PaymentFrequency.ThreeMonths, Text = PaymentFrequency.ThreeMonths.GetDescription() },
        new() { Value = PaymentFrequency.SixMonths,   Text = PaymentFrequency.SixMonths.GetDescription() },
        new() { Value = PaymentFrequency.Yearly,      Text = PaymentFrequency.Yearly.GetDescription() },
        new() { Value = PaymentFrequency.TwoYear,     Text = PaymentFrequency.TwoYear.GetDescription() },
        new() { Value = PaymentFrequency.ThreeYear,   Text = PaymentFrequency.ThreeYear.GetDescription() },
        new() { Value = PaymentFrequency.FourYear,    Text = PaymentFrequency.FourYear.GetDescription() },
        new() { Value = PaymentFrequency.FiveYear,    Text = PaymentFrequency.FiveYear.GetDescription() }
    };

    public class PaymentFrequencyOption
    {
        public PaymentFrequency Value { get; set; }
        public string Text { get; set; }
    }

    public static List<DayCountConvOption> GetDayCountConvOptions() => new()
    {
        new() { Value = DayCountConv.ACT_360,      Text = DayCountConv.ACT_360.GetDescription() },
        new() { Value = DayCountConv.ACT_365Fixed, Text = DayCountConv.ACT_365Fixed.GetDescription() },
        new() { Value = DayCountConv.ACT_ISDA,     Text = DayCountConv.ACT_ISDA.GetDescription() },
        new() { Value = DayCountConv._30_360,      Text = DayCountConv._30_360.GetDescription() },
        new() { Value = DayCountConv._30E_360,     Text = DayCountConv._30E_360.GetDescription() }
    };

    public class DayCountConvOption
    {
        public DayCountConv Value { get; set; }
        public string Text { get; set; }
    }

    private static List<PayBusDayConvOption> GetPayBusDayConvOptions() => new()
    {
        new() { Value = PayBusDayConv.Following,    Text = PayBusDayConv.Following.GetDescription() },
        new() { Value = PayBusDayConv.ModFollowing, Text = PayBusDayConv.ModFollowing.GetDescription() },
        new() { Value = PayBusDayConv.Preceding,    Text = PayBusDayConv.Preceding.GetDescription() },
        new() { Value = PayBusDayConv.ModPreceding, Text = PayBusDayConv.ModPreceding.GetDescription() },
        new() { Value = PayBusDayConv.FRN,          Text = PayBusDayConv.FRN.GetDescription() }
    };

    public class PayBusDayConvOption
    {
        public PayBusDayConv Value { get; set; }
        public string Text { get; set; }
    }

    public static List<AmortizationMethodOption> GetAmortizationMethodOptions() => new()
    {
        new() { Value = AmortizationMethod.None, Text = AmortizationMethod.None.GetDescription() },
        new() { Value = AmortizationMethod.TotalCashFlowMethod, Text = AmortizationMethod.TotalCashFlowMethod.GetDescription() },
        new() { Value = AmortizationMethod.Straightline, Text = AmortizationMethod.Straightline.GetDescription() },
        new() { Value = AmortizationMethod.IntrinsicValueMethod, Text = AmortizationMethod.IntrinsicValueMethod.GetDescription() },
        new() { Value = AmortizationMethod.Swaplet, Text = AmortizationMethod.Swaplet.GetDescription() }
    };

    public class AmortizationMethodOption
    {
        public AmortizationMethod Value { get; set; }
        public string Text { get; set; }
    }

    public static List<IntrinsicAmortizationMethodOption> GetIntrinsicAmortizationMethodOptions() => new()
    {
        new() { Value = AmortizationMethod.None, Text = AmortizationMethod.None.GetDescription() },
        new() { Value = AmortizationMethod.TotalCashFlowMethod, Text = AmortizationMethod.TotalCashFlowMethod.GetDescription() },
        new() { Value = AmortizationMethod.Straightline, Text = AmortizationMethod.Straightline.GetDescription() },
        new() { Value = AmortizationMethod.IntrinsicValueMethod, Text = AmortizationMethod.IntrinsicValueMethod.GetDescription() },
        new() { Value = AmortizationMethod.Swaplet, Text = AmortizationMethod.Swaplet.GetDescription() }
    };

    public class IntrinsicAmortizationMethodOption
    {
        public AmortizationMethod Value { get; set; }
        public string Text { get; set; }
    }
    #endregion
}
