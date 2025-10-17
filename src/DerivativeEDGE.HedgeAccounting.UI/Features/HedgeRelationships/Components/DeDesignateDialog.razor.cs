namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class DeDesignateDialog
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnDeDesignated { get; set; }

    // Model properties
    [Parameter] public int DedesignationReason { get; set; } = 0;
    [Parameter] public DateTime? DedesignationDate { get; set; }
    [Parameter] public int CashPaymentType { get; set; } = 0;
    [Parameter] public decimal? Payment { get; set; } = 0;
    [Parameter] public decimal? Accrual { get; set; } = 0;
    [Parameter] public decimal? BasisAdjustment { get; set; } = 0;
    [Parameter] public decimal? BasisAdjustmentBalance { get; set; } = 0;
    [Parameter] public bool ShowBasisAdjustmentBalance { get; set; }
    [Parameter] public DateTime? TimeValuesStartDate { get; set; }
    [Parameter] public DateTime? TimeValuesEndDate { get; set; }
    [Parameter] public bool HedgedExposureExist { get; set; } = true;

    // UI State
    [Parameter] public string UserMessage { get; set; } = string.Empty;
    [Parameter] public bool IsError { get; set; }
    [Parameter] public bool IsDeDesignateDisabled { get; set; }

    // Computed properties for field states
    private bool PaymentFieldsEnabled => true; // Per DE-3277, always enabled
    private bool DateFieldsEnabled => HedgedExposureExist;

    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private async Task HandleDeDesignate()
    {
        if (OnDeDesignated.HasDelegate)
        {
            await OnDeDesignated.InvokeAsync();
        }
        await HandleClose();
    }

    private async Task HandleReasonChange(int reason)
    {
        DedesignationReason = reason;

        // API Call: Load de-designation data for the selected reason
        // await Mediator.Send(new GetDedesignateData.Query(HedgeId, reason));
        // Update model properties based on response

        StateHasChanged();
    }

    private void HandleCashPaymentTypeChange(int cashPaymentType)
    {
        CashPaymentType = cashPaymentType;
        StateHasChanged();
    }

    private void HandleHedgedExposureChange(bool exists)
    {
        HedgedExposureExist = exists;
        StateHasChanged();
    }
}
