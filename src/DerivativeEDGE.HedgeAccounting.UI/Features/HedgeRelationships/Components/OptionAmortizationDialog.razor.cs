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
    [Parameter] public DerivativeEDGEHAEntityValueObjectsOptionAmortizationDefaultValues DefaultValues { get; set; }
    #endregion

    #region Injected Services
    [Inject] private IAlertService AlertService { get; set; }
    [Inject] private IMediator Mediator { get; set; }
    #endregion

    #region Private Properties
    public DocumentContent Model { get; set; } = new();
    // UI-only property that controls whether the Generate button is enabled (not persisted to database)
    // Legacy: hr_hedgeRelationshipAddEditCtrl.js lines 1032, 3325 and optionTimeValue.cshtml line 149
    private bool AmortizeOptionPremium { get; set; } = true;
    
    // Loading state for save operation
    private bool IsSavingOptionAmortization { get; set; } = false;
    
    // Legacy behavior: Generate/Update button is disabled when validation fails
    // Required fields: AmortizeOptionPremium checked, AmortizationMethod not None, GLAccountID, StartDate, EndDate
    // Legacy: optionTimeValue.cshtml lines 148-151
    private bool IsGenerateDisabled => 
        IsSavingOptionAmortization ||
        !AmortizeOptionPremium ||
        OptionAmortizationModel.AmortizationMethod == DerivativeEDGEHAEntityEnumAmortizationMethod.None ||
        OptionAmortizationModel.GLAccountID == 0 ||
        string.IsNullOrEmpty(OptionAmortizationModel.StartDate) ||
        string.IsNullOrEmpty(OptionAmortizationModel.EndDate);
    
    // Intrinsic Value fields - These exist on Entity but not on ViewModel
    // Store locally and map to Entity when submitting
    // Legacy: optionTimeValue.cshtml lines 63-117
    private long IVGLAccountID { get; set; }
    private long IVContraAccountID { get; set; }
    private DerivativeEDGEHAEntityEnumAmortizationMethod IVAmortizationMethod { get; set; } = DerivativeEDGEHAEntityEnumAmortizationMethod.None;
    private double IntrinsicValue { get; set; }
    
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

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        // Legacy behavior (hr_hedgeRelationshipAddEditCtrl.js):
        // - When creating new (line 3325): AmortizeOptionPremimum = true
        // - When editing (line 1032): AmortizeOptionPremimum = IsAnOptionHedge
        // NOTE: This is a UI-only property, not persisted to the database
        if (OptionAmortizationModel?.ID > 0)
        {
            // Editing existing entry - set from IsAnOptionHedge (legacy: line 1032)
            AmortizeOptionPremium = IsAnOptionHedge;
            // Note: Intrinsic value fields are only shown when ID === 0 (legacy: optionTimeValue.cshtml lines 63, 81, 98, 113)
            // So we don't need to load them when editing
        }
        else
        {
            // Creating new entry - default to true (legacy: line 3325)
            AmortizeOptionPremium = true;
            
            // Reset intrinsic value fields for new entry
            if (DefaultValues != null)
            {
                IVGLAccountID = DefaultValues.GlAccountId2;
                IVContraAccountID = DefaultValues.GlContraAcctId2;
                IVAmortizationMethod = DefaultValues.IVAmortizationMethod;
                IntrinsicValue = DefaultValues.IntrinsicValue;
            }
        }
    }
    #endregion
    
    #region Helper Methods
    private List<AmortizationMethodOption> GetFilteredAmortizationMethodOptions()
    {
        // Legacy: filterByOptionAmortType (hr_hedgeRelationshipAddEditCtrl.js line 1104-1106)
        // Exclude "Swaplet" when OptionTimeValueAmortType === "OptionTimeValue"
        if (OptionAmortizationModel?.OptionTimeValueAmortType == DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.OptionTimeValue)
        {
            return AmortizationMethodOptions
                .Where(option => option.Value != AmortizationMethod.Swaplet)
                .ToList();
        }
        
        return AmortizationMethodOptions;
    }
    #endregion

    #region Event Handlers
    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private async Task OnSubmitOptionAmortization()
    {
        try
        {
            IsSavingOptionAmortization = true;
            StateHasChanged();

            OptionAmortizationModel.HedgeRelationshipID = HedgeRelationship.ID;
            OptionAmortizationModel.OptionTimeValueAmortType = DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.OptionTimeValue;
            // NOTE: AmortizeOptionPremium is a UI-only property and is not persisted to the database

            var isUpdate = OptionAmortizationModel.ID > 0;
            var successMessage = isUpdate ? "Success! Option Amortization Updated." : "Success! Option Amortization Created.";

            // Map the local intrinsic value properties to a custom command that includes them
            // These fields exist on Entity but not on ViewModel, so we pass them separately
            var response = await Mediator.Send(new CreateHedgeRelationshipOptionTimeValueAmort.Command(
                OptionAmortizationModel, 
                HedgeRelationship,
                IsAnOptionHedge,
                IVGLAccountID,
                IVContraAccountID,
                IVAmortizationMethod,
                IntrinsicValue));

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
        finally
        {
            IsSavingOptionAmortization = false;
            StateHasChanged();
        }
    }
    #endregion
}
