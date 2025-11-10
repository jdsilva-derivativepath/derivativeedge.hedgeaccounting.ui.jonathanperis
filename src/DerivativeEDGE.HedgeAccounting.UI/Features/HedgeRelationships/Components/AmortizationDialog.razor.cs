namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class AmortizationDialog
{
    #region Parameters
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnAmortizationSaved { get; set; }

    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM AmortizationModel { get; set; } = new();
    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    
    [Parameter] public List<DerivativeEDGEHAEntityGLAccount> AmortizationGLAccounts { get; set; } = [];
    [Parameter] public List<DerivativeEDGEHAEntityGLAccount> AmortizationContraAccounts { get; set; } = [];
    [Parameter] public List<FinancialCenterOption> FinancialCenterOptions { get; set; } = [];
    [Parameter] public List<PaymentFrequencyOption> PaymentFrequencyOptions { get; set; } = [];
    [Parameter] public List<DayCountConvOption> DayCountConvOptions { get; set; } = [];
    [Parameter] public List<PayBusDayConvOption> PayBusDayConvOptions { get; set; } = [];
    #endregion

    #region Injected Services
    [Inject] private IAlertService AlertService { get; set; }
    [Inject] private IMediator Mediator { get; set; }
    #endregion

    #region Private Properties
    private List<string> AmortizationFinancialCenters { get; set; }
    
    // Legacy behavior: Save button is disabled when required fields are not filled
    // Required fields: GLAccountID, ContraAccountID, StartDate, EndDate, FrontRollDate, BackRollDate
    private bool IsSaveDisabled => 
        AmortizationModel.GLAccountID == 0 ||
        AmortizationModel.ContraAccountID == 0 ||
        string.IsNullOrEmpty(AmortizationModel.StartDate) ||
        string.IsNullOrEmpty(AmortizationModel.EndDate) ||
        string.IsNullOrEmpty(AmortizationModel.FrontRollDate) ||
        string.IsNullOrEmpty(AmortizationModel.BackRollDate);
    
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
    #endregion

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        // Initialize AmortizationFinancialCenters from model when dialog opens
        if (AmortizationModel?.FinancialCenters != null)
        {
            AmortizationFinancialCenters = AmortizationModel.FinancialCenters
                .Select(fc => fc.ToString())
                .ToList();
        }
        else
        {
            AmortizationFinancialCenters = [];
        }
    }
    #endregion

    #region Event Handlers
    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private async Task OnSubmitAmortization()
    {
        try
        {
            AmortizationModel.HedgeRelationshipID = HedgeRelationship.ID;
            AmortizationModel.OptionTimeValueAmortType = DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.Amortization;

            if (AmortizationFinancialCenters != null)
            {
                AmortizationModel.FinancialCenters = [.. AmortizationFinancialCenters
                    .Select(s => Enum.TryParse<DerivativeEDGEDomainEntitiesEnumsFinancialCenter>(s, out var result) ? result : default)
                    .Where(fc => fc != default)];
            }

            var isUpdate = AmortizationModel.ID > 0;
            var successMessage = isUpdate ? "Success! Amortization Updated." : "Success! Amortization Created.";

            var response = await Mediator.Send(new CreateHedgeRelationshipOptionTimeValueAmort.Command(AmortizationModel, HedgeRelationship));

            await AlertService.ShowToast(successMessage, AlertKind.Success, "Success", showButton: true);

            if (OnAmortizationSaved.HasDelegate)
            {
                await OnAmortizationSaved.InvokeAsync();
            }

            await HandleClose();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error saving Amortization: {ex.InnerException.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }

    private void OnAmortizationComboBoxCreated(object args)
    {
        // When opening modal for new entry (ID = 0), GLAccountID and ContraAccountID will be 0
        // which corresponds to "None" option, so no need to override
        // When editing existing entry, values are already set from the model
        
        // Legacy behavior: first item was selected by default, but now we default to "None" (ID = 0)
        // This matches the legacy system where <option value="">None</option> was the default
    }
    #endregion
}
