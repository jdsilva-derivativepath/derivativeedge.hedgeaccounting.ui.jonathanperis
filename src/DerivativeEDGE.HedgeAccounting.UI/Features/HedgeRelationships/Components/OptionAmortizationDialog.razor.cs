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
    private bool AmortizeOptionPremium { get; set; }
    
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

    private void OnOptionAmortizationComboBoxCreated(object args)
    {
        // Set the first item as the default when available
        if (AmortizationGLAccounts?.Any() == true && OptionAmortizationModel.GLAccountID == 0)
        {
            OptionAmortizationModel.GLAccountID = AmortizationGLAccounts.First().Id;
            StateHasChanged();
        }

        // Set the first item as the default when available
        if (AmortizationContraAccounts?.Any() == true && OptionAmortizationModel.ContraAccountID == 0)
        {
            OptionAmortizationModel.ContraAccountID = AmortizationContraAccounts.First().Id;
            StateHasChanged();
        }
    }
    #endregion
}
