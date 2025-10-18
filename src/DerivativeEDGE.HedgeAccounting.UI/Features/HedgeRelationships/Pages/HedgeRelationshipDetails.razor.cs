namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages;

public partial class HedgeRelationshipDetails
{
    #region Parameters
    [Parameter] public long HedgeRelationshipId { get; set; }
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

    private bool IsNewHedgeDocumentTemplate => HedgeRelationshipDataHelper.IsNewHedgeDocumentTemplate(HedgeRelationship);
    #endregion

    #region Private Properties
    private DateTime? CurveDate { get; set; } = DateTime.Today;
    private DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    private string OpenModal { get; set; } = string.Empty;
    private SfTab hedgerelationshiptabRef;
    private InstrumentAnalysisTab instrumentAnalysisTabRef;
    private TestResultsTab testResultsTabRef;
    private List<string> ValidationErrors { get; set; } = [];
    private bool IsDpiUser { get; set; }
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
    #endregion

    #region Data Collections
    public List<Client> AvailableClients { get; private set; } = [];
    public List<DerivativeEDGEHAEntityLegalEntity> AvailableEntities { get; private set; } = [];

    public List<DerivativeEDGEHAEntityGLAccount> AmortizationGLAccounts { get; private set; } = [];
    public List<DerivativeEDGEHAEntityGLAccount> AmortizationContraAccounts { get; private set; } = [];

    public List<DerivativeEDGEHAEntityGLAccount> OptionAmortizationGLAccounts { get; private set; } = [];
    public List<DerivativeEDGEHAEntityGLAccount> OptionAmortizationContraAccounts { get; private set; } = [];

    public List<DerivativeEDGEHAEntityGLAccount> IntrinsicAmortizationGLAccounts { get; private set; } = [];
    public List<DerivativeEDGEHAEntityGLAccount> IntrinsicAmortizationContraAccounts { get; private set; } = [];

    // Enum‐based data sources
    public List<FinancialCenterOption> AvailableFinancialCenters { get; private set; }
        = HedgeRelationshipDataHelper.GetFinancialCenterOptions();
    public List<PaymentFrequencyOption> AvailablePaymentFrequencies { get; private set; }
        = HedgeRelationshipDataHelper.GetPaymentFrequencyOptions();
    public List<DayCountConvOption> AvailableDayCountConventions { get; private set; }
        = HedgeRelationshipDataHelper.GetDayCountConvOptions();
    public List<PayBusDayConvOption> AvailablePayBusDayConventions { get; private set; }
        = HedgeRelationshipDataHelper.GetPayBusDayConvOptions();
    #endregion

    #region Loading States
    public bool IsLoadingHedgeRelationship { get; set; }
    public bool IsLoadingClients { get; set; }
    public bool IsLoadingEntities { get; set; }
    public bool IsGeneratingInceptionPackage { get; set; }
    public bool IsDownloadingSpecsAndChecks { get; set; }
    public bool IsSavingHedgeRelationship { get; set; }
    public bool IsRunningRegression { get; set; }

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
    
    private string _templateDisplayName = string.Empty;
    private string TemplateDisplayName
    {
        get
        {
            if (HedgeRelationship?.InceptionMemoTemplateID == null)
                return string.Empty;

            var templates = DropdownDataHelper.GetDropdownDatasource("hedgingobjective");
            var selectedTemplate = templates.FirstOrDefault(t => t.ID == HedgeRelationship.InceptionMemoTemplateID);
            _templateDisplayName = selectedTemplate?.Text ?? string.Empty;
            return _templateDisplayName;
        }
        set => _templateDisplayName = value ?? string.Empty;
    }

    // TODO: Replace with actual business logic
    private bool IsHedgeDesignated => false; // Replace with real data check

    // Individual checkbox states - these should be bound to specific properties in the model
    private bool QualitativeAssessment { get; set; }
    private bool Acquisition { get; set; }
    private bool AdjustDates { get; set; }
    private bool Straightline { get; set; }
    private bool IncludeInRegression { get; set; }

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
    private readonly List<DropDownMenuItem> WorkflowItems = [];

    // Dynamic chart data from HedgeRegressionBatches
    private List<ChartDataModel> EffectivenessChartData { get; set; } = [];

    private readonly List<ToolbarItemModel> BasicTools =
    [
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
    ];
    #endregion

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        BuildWorkflowItems();
    }

    protected override async Task OnInitializedAsync()
    {
        // Initialize IsDpiUser from UserAuthData
        IsDpiUser = UserAuthData?.IsDpiUser ?? false;

        // Load hedge relationship first to ensure data is populated
        await GetHedgeRelationship(HedgeRelationshipId);

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
        EffectivenessChartData = HedgeRelationshipDataHelper.GenerateEffectivenessChartData(HedgeRelationship);
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
            AvailableEntities = [.. response.Entities.Select(data => new DerivativeEDGEHAEntityLegalEntity
            {
                Id = data.EntityId,
                Name = data.EntityLongName,
            })];

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
            // Dedesignated state: Show Redraft and De-Designate (DE-2731)
            // Old JS logic: removes "Re-Designate", then replaces "Designate" with "Redraft", leaving ["Redraft", "De-Designate"]
            WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
            WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasWorkflowPermission });
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
        // Append HedgeRelationshipId and ClientId parameters as done in legacy system
        var url = $"{pathAndQuery}HedgeRelationshipId={HedgeRelationshipId}&ClientId={HedgeRelationship.ClientID}";
        NavManager.NavigateTo(url, forceLoad: true);
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

            // Use DotNetStreamReference for proper binary file download
            using var streamRef = new DotNetStreamReference(stream: result.ExcelStream);
            await JSRuntime.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);

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

            // Use DotNetStreamReference for proper binary file download
            using var streamRef = new DotNetStreamReference(stream: result.ExcelStream);
            await JSRuntime.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);

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

            // Pre-Issuance Hedge logic: Only applicable to CashFlow hedge types
            // For FairValue and NetInvestment hedge types, PreIssuanceHedge must be false
            if (HedgeRelationship.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue ||
                HedgeRelationship.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
            {
                HedgeRelationship.PreIssuanceHedge = false;
            }

            // Portfolio Layer Method logic: Only applicable to FairValue hedge types
            // For CashFlow and NetInvestment hedge types, PortfolioLayerMethod must be false
            if (HedgeRelationship.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow ||
                HedgeRelationship.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
            {
                HedgeRelationship.PortfolioLayerMethod = false;
            }

            var result = await Mediator.Send(new UpdateHedgeRelationship.Command(HedgeRelationship));
            if (!result.HasError)
            {
                // Reload from backend to ensure all fields are correct
                await GetHedgeRelationship(HedgeRelationshipId);
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
        if (ValidationErrors.Count > 0)
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
                if (regressionResponse.ValidationErrors?.Count > 0)
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
                await GetHedgeRelationship(HedgeRelationshipId);

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

        // Parse string role ID to integer and cast to EdgeRole enum
        if (!int.TryParse(role, out var roleId))
            return false;

        var edgeRole = (DerivativeEDGE.Authorization.AuthClaims.EdgeRole)roleId;
        return UserAuthData.Roles.Contains(edgeRole);
    }

    private bool HasRequiredRole() =>
        CheckUserRole("24") || CheckUserRole("17") || CheckUserRole("5");

    private bool CanEditCheckbox() =>
        HedgeRelationship?.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft || CheckUserRole("24");

    private bool CanEditPreIssuanceHedge() =>
        HedgeRelationship?.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft;

    private bool CanEditPortfolioLayerMethod() =>
        HedgeRelationship?.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft;

    private bool CanEditFairValueMethod() =>
        HedgeRelationship?.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft;

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

    private bool IsHedgingInstrumentStructureDisabled()
    {
        if (HedgeRelationship == null)
            return true;

        // Disable the Hedging Instrument Structure dropdown when in Designated or Dedesignated status
        return HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Designated ||
               HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Dedesignated;
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
            IsRunningRegression = true;
            StateHasChanged();

            // Run the backload regression analysis
            var response = await Mediator.Send(new RunRegression.Command(HedgeRelationship, DerivativeEDGEHAEntityEnumHedgeResultType.Backload));

            if (response.HasError)
            {
                if (response.ValidationErrors?.Count > 0)
                {
                    var errorMessage = string.Join("; ", response.ValidationErrors);
                    await AlertService.ShowToast($"Backload failed: {errorMessage}", AlertKind.Error, "Backload Error", showButton: true);
                }
                else
                {
                    await AlertService.ShowToast(response.ErrorMessage ?? "Failed to create Backload", AlertKind.Error, "Error", showButton: true);
                }
                return;
            }

            // Update the hedge relationship with the new data
            if (response.Data != null)
            {
                HedgeRelationship = response.Data;

                // Regenerate effectiveness chart data
                GenerateEffectivenessChartData();

                // Refresh the Instruments and Analysis tab if it's loaded
                if (instrumentAnalysisTabRef != null)
                {
                    await instrumentAnalysisTabRef.RefreshGridData();
                }

                // Refresh the TestResultsTab with new data
                if (testResultsTabRef != null)
                {
                    await testResultsTabRef.RefreshTestResultsData();
                }

                await AlertService.ShowToast("Backload completed successfully!", AlertKind.Success, "Success", showButton: true);

                // Switch to the "Test Results" tab (Tab index 1 - Test Results)
                if (hedgerelationshiptabRef != null)
                {
                    await hedgerelationshiptabRef.SelectAsync(1);
                }
            }
            else
            {
                // If no data returned, refresh the hedge relationship from the API
                await GetHedgeRelationship(HedgeRelationshipId);

                // Refresh the TestResultsTab after API refresh
                if (testResultsTabRef != null)
                {
                    await testResultsTabRef.RefreshTestResultsData();
                }

                await AlertService.ShowToast("Backload completed successfully!", AlertKind.Success, "Success", showButton: true);

                // Switch to the "Test Results" tab
                if (hedgerelationshiptabRef != null)
                {
                    await hedgerelationshiptabRef.SelectAsync(1);
                }
            }
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Failed to create Backload: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsRunningRegression = false;
            StateHasChanged();
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
        if (ValidationErrors.Count > 0)
        {
            StateHasChanged();
            return;
        }

        try
        {
            // Check if document template exists (matching legacy behavior)
            var documentTemplateResponse = await Mediator.Send(
                new FindDocumentTemplate.Query(HedgeRelationshipId));

            if (documentTemplateResponse.HasError)
            {
                await AlertService.ShowToast(documentTemplateResponse.ErrorMessage, AlertKind.Error, "Designation Failed", showButton: true);
                return;
            }

            // If document template exists, save current state before designation (legacy: submit → init → designate)
            if (documentTemplateResponse.HasTemplate)
            {
                await SaveHedgeRelationshipAsync();
                
                // Reload the hedge relationship after save
                await GetHedgeRelationship(HedgeRelationshipId);
            }

            // Execute designation workflow
            var response = await Mediator.Send(new DesignateHedgeRelationship.Command(HedgeRelationshipId));
            
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
                new GetDeDesignateData.Query(HedgeRelationshipId, (DerivativeEDGEHAEntityEnumDedesignationReason)reason));
            
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
                DedesignateAccrual = response.Accrual;
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
                HedgeRelationshipId: HedgeRelationshipId,
                DedesignationDate: DedesignationDateDialog.GetValueOrDefault(),
                DedesignationReason: DedesignationReason,
                Payment: DedesignatePayment.GetValueOrDefault(),
                TimeValuesStartDate: DedesignateTimeValuesStartDate.GetValueOrDefault(),
                TimeValuesEndDate: DedesignateTimeValuesEndDate.GetValueOrDefault(),
                CashPaymentType: CashPaymentType,
                HedgedExposureExist: HedgedExposureExist,
                BasisAdjustment: BasisAdjustment.GetValueOrDefault(),
                BasisAdjustmentBalance: BasisAdjustmentBalance.GetValueOrDefault());
            
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
            var response = await Mediator.Send(new RedraftHedgeRelationship.Command(HedgeRelationshipId));
            
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
            var findDocTemplateResponse = await Mediator.Send(new FindDocumentTemplate.Query(HedgeRelationshipId));
            
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
                await GetHedgeRelationship(HedgeRelationshipId);
            }

            // Get re-designation data
            var redesignateResponse = await Mediator.Send(new GetReDesignateData.Query(HedgeRelationshipId));
            
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
                HedgeRelationshipId: HedgeRelationshipId,
                RedesignationDate: RedesignationDate.GetValueOrDefault(),
                Payment: RedesignatePayment.GetValueOrDefault(),
                TimeValuesStartDate: RedesignateTimeValuesStartDate.GetValueOrDefault(),
                TimeValuesEndDate: RedesignateTimeValuesEndDate.GetValueOrDefault(),
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

    #region Modal Callbacks
    private async Task OnAmortizationSaved()
    {
        // Refresh the hedge relationship data after saving amortization
        await GetHedgeRelationship(HedgeRelationshipId);
        StateHasChanged();
    }

    private async Task OnOptionAmortizationSaved()
    {
        // Refresh the hedge relationship data after saving option amortization
        await GetHedgeRelationship(HedgeRelationshipId);
        StateHasChanged();
    }
    #endregion

    #region Checkbox Event Handlers
    private void OnIsAnOptionHedgeChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        if (HedgeRelationship != null)
        {
            // Legacy rule: IsAnOptionHedge and OffMarket are mutually exclusive
            if (args.Checked && HedgeRelationship.OffMarket)
            {
                // Prevent checking the box if OffMarket is true
                HedgeRelationship.IsAnOptionHedge = false;
                return;
            }
            
            HedgeRelationship.IsAnOptionHedge = args.Checked;
            
            // Legacy rule: When IsAnOptionHedge is unchecked, also clear related option hedge fields
            if (!args.Checked)
            {
                HedgeRelationship.AmortizeOptionPremimum = false;
                HedgeRelationship.IsDeltaMatchOption = false;
                HedgeRelationship.ExcludeIntrinsicValue = false;
            }
            
            StateHasChanged();
        }
    }
    
    private void OnOffMarketChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        if (HedgeRelationship != null)
        {
            // Legacy rule: IsAnOptionHedge and OffMarket are mutually exclusive
            if (args.Checked && HedgeRelationship.IsAnOptionHedge)
            {
                // Prevent checking the box if IsAnOptionHedge is true
                HedgeRelationship.OffMarket = false;
                return;
            }
            
            HedgeRelationship.OffMarket = args.Checked;
            StateHasChanged();
        }
    }
    
    private void OnExcludeIntrinsicValueChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        if (HedgeRelationship != null)
        {
            // Legacy rule: ExcludeIntrinsicValue can only be true when IsAnOptionHedge is true
            if (args.Checked && !HedgeRelationship.IsAnOptionHedge)
            {
                // Prevent checking the box if not an option hedge
                HedgeRelationship.ExcludeIntrinsicValue = false;
            }
            else
            {
                HedgeRelationship.ExcludeIntrinsicValue = args.Checked;
            }
            
            if (!HedgeRelationship.ExcludeIntrinsicValue)
            {
                // When unchecked, reset IntrinsicMethod to None and related options
                HedgeRelationship.IntrinsicMethod = DerivativeEDGEHAEntityEnumIntrinsicMethod.None;
                HedgeRelationship.AmortizeOptionPremimum = false;
                HedgeRelationship.IsDeltaMatchOption = false;
            }
            else
            {
                // When checked, ensure IntrinsicMethod has a valid value
                // Default to None if it's not already set to a valid method
                if (HedgeRelationship.IntrinsicMethod == DerivativeEDGEHAEntityEnumIntrinsicMethod.None || 
                    HedgeRelationship.IntrinsicMethod == default)
                {
                    HedgeRelationship.IntrinsicMethod = DerivativeEDGEHAEntityEnumIntrinsicMethod.None;
                }
            }
            
            StateHasChanged();
        }
    }
    #endregion
}
