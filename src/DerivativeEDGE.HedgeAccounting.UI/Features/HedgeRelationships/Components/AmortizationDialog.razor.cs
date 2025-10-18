namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class AmortizationDialog : IDisposable
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
    private AmortizationFormModel FormModel { get; set; } = new();
    private EditContext? EditContext { get; set; }
    private bool IsSaveButtonDisabled => EditContext == null || !EditContext.Validate();
    #endregion

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        // Convert API model to form model for validation
        if (AmortizationModel != null)
        {
            FormModel = AmortizationFormModel.FromApiModel(AmortizationModel);
            
            // Initialize FinancialCenters from API model
            if (AmortizationModel.FinancialCenters != null)
            {
                FormModel.FinancialCenters = AmortizationModel.FinancialCenters
                    .Select(fc => fc.ToString())
                    .ToList();
            }
            else
            {
                FormModel.FinancialCenters = [];
            }
        }
        else
        {
            FormModel = new AmortizationFormModel();
        }
        
        // Create EditContext for validation
        EditContext = new EditContext(FormModel);
        EditContext.OnFieldChanged += OnFieldChanged;
    }
    
    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        // Trigger re-render when a field changes to update save button state
        StateHasChanged();
    }
    
    public void Dispose()
    {
        if (EditContext != null)
        {
            EditContext.OnFieldChanged -= OnFieldChanged;
        }
    }
    #endregion

    #region Event Handlers
    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private async Task OnSubmitAmortization(EditContext context)
    {
        try
        {
            // Validate form before submission
            if (!context.Validate())
            {
                await AlertService.ShowToast("Please fill in all required fields.", AlertKind.Warning, "Validation Error", showButton: true);
                return;
            }
            
            // Convert form model back to API model
            var apiModel = FormModel.ToApiModel();
            apiModel.HedgeRelationshipID = HedgeRelationship.ID;
            apiModel.OptionTimeValueAmortType = DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.Amortization;

            if (FormModel.FinancialCenters != null)
            {
                apiModel.FinancialCenters = [.. FormModel.FinancialCenters
                    .Select(s => Enum.TryParse<DerivativeEDGEDomainEntitiesEnumsFinancialCenter>(s, out var result) ? result : default)
                    .Where(fc => fc != default)];
            }

            var isUpdate = FormModel.ID > 0;
            var successMessage = isUpdate ? "Success! Amortization Updated." : "Success! Amortization Created.";

            var response = await Mediator.Send(new CreateHedgeRelationshipOptionTimeValueAmort.Command(apiModel, HedgeRelationship));

            await AlertService.ShowToast(successMessage, AlertKind.Success, "Success", showButton: true);

            if (OnAmortizationSaved.HasDelegate)
            {
                await OnAmortizationSaved.InvokeAsync();
            }

            await HandleClose();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"Error saving Amortization: {ex.Message}", AlertKind.Error, "Error", showButton: true);
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
    
    private string GetSaveButtonText()
    {
        return FormModel.ID > 0 ? "Update" : "Save";
    }
    #endregion
}
