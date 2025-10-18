namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class OptionAmortizationDialog
{
    #region Parameters
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnOptionAmortizationSaved { get; set; }

    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM OptionAmortizationModel { get; set; } = new();
    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    [Parameter] public bool IsAnOptionHedge { get; set; }
    
    [Parameter] public List<DerivativeEDGEHAEntityGLAccount> AmortizationGLAccounts { get; set; } = [];
    [Parameter] public List<DerivativeEDGEHAEntityGLAccount> AmortizationContraAccounts { get; set; } = [];
    [Parameter] public List<AmortizationMethodOption> AmortizationMethodOptions { get; set; } = [];
    #endregion

    #region Injected Services
    [Inject] private IAlertService AlertService { get; set; }
    [Inject] private IMediator Mediator { get; set; }
    #endregion

    #region Private Properties
    public DocumentContent Model { get; set; } = new();
    
    // Legacy: AmortizeOptionPremimum defaults to true (line 3325 in legacy openOptionTimeValueAmortDialog)
    // For new records, defaults to true. For existing records being edited, set to IsAnOptionHedge (line 1032)
    private bool AmortizeOptionPremium { get; set; } = true;
    
    // Legacy: GL Account and Contra dropdown lists with "None" option (lines 55, 76 in legacy optionTimeValue.cshtml)
    private List<DerivativeEDGEHAEntityGLAccount> GLAccountsWithNone => 
        [new DerivativeEDGEHAEntityGLAccount { Id = 0, AccountDescription = "None" }, .. AmortizationGLAccounts];
    
    private List<DerivativeEDGEHAEntityGLAccount> ContraAccountsWithNone => 
        [new DerivativeEDGEHAEntityGLAccount { Id = 0, AccountDescription = "None" }, .. AmortizationContraAccounts];
    
    // Legacy: Filter out "Swaplet" when OptionTimeValueAmortType is "OptionTimeValue" (filterByOptionAmortType function)
    private List<AmortizationMethodOption> FilteredAmortizationMethodOptions =>
        AmortizationMethodOptions
            .Where(option => !(OptionAmortizationModel.OptionTimeValueAmortType == DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.OptionTimeValue 
                            && option.Value == DerivativeEDGEHAEntityEnumAmortizationMethod.Swaplet))
            .ToList();
    
    // Legacy: Generate button disabled logic (lines 149-151 in optionTimeValue.cshtml)
    // Disabled when: !AmortizeOptionPremimum OR AmortizationMethod === 'None' OR form is invalid
    private bool IsGenerateButtonDisabled => 
        !AmortizeOptionPremium || 
        OptionAmortizationModel?.AmortizationMethod == DerivativeEDGEHAEntityEnumAmortizationMethod.None ||
        OptionAmortizationModel?.GLAccountID == 0 || // Required field
        string.IsNullOrEmpty(OptionAmortizationModel?.StartDate) || // Required field
        string.IsNullOrEmpty(OptionAmortizationModel?.EndDate); // Required field
    
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

    #region Event Handlers
    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private async Task OnSubmitOptionAmortization(EditContext context)
    {
        try
        {
            OptionAmortizationModel.HedgeRelationshipID = HedgeRelationship.ID;
            OptionAmortizationModel.OptionTimeValueAmortType = DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.OptionTimeValue;

            var isUpdate = OptionAmortizationModel.ID > 0;
            var successMessage = isUpdate ? "Success! Option Amortization Updated." : "Success! Option Amortization Created.";

            var response = await Mediator.Send(new CreateHedgeRelationshipOptionTimeValueAmort.Command(OptionAmortizationModel, HedgeRelationship));

            await AlertService.ShowToast(successMessage, AlertKind.Success, "Success", showButton: true);

            if (OnOptionAmortizationSaved.HasDelegate)
            {
                await OnOptionAmortizationSaved.InvokeAsync();
            }

            await HandleClose();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error saving Option Amortization: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
    }
    
    protected override void OnParametersSet()
    {
        // Legacy: Set defaults when no user data exists (GLAccountID and ContraAccountID default to 0/"None")
        // If the model has no ID (new record) and no GL Account or Contra selected, they default to 0 (None)
        if (OptionAmortizationModel?.ID == 0)
        {
            // Ensure defaults are set to 0 (None) if not already set by parent
            if (OptionAmortizationModel.GLAccountID == 0)
            {
                OptionAmortizationModel.GLAccountID = 0;
            }
            
            if (OptionAmortizationModel.ContraAccountID == 0)
            {
                OptionAmortizationModel.ContraAccountID = 0;
            }
        }
    }
    #endregion
}
