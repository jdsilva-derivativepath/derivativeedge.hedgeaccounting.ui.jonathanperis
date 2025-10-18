namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class DeDesignateDialog
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnDeDesignated { get; set; }
    [Parameter] public EventCallback<int> OnReasonChanged { get; set; }

    // Model properties
    [Parameter] public int DedesignationReason { get; set; } = 0;
    [Parameter] public EventCallback<int> DedesignationReasonChanged { get; set; }
    [Parameter] public DateTime? DedesignationDate { get; set; }
    [Parameter] public EventCallback<DateTime?> DedesignationDateChanged { get; set; }
    [Parameter] public int CashPaymentType { get; set; } = 0;
    [Parameter] public EventCallback<int> CashPaymentTypeChanged { get; set; }
    [Parameter] public decimal? Payment { get; set; } = 0;
    [Parameter] public EventCallback<decimal?> PaymentChanged { get; set; }
    [Parameter] public decimal? Accrual { get; set; } = 0;
    [Parameter] public decimal? BasisAdjustment { get; set; } = 0;
    [Parameter] public decimal? BasisAdjustmentBalance { get; set; } = 0;
    [Parameter] public bool ShowBasisAdjustmentBalance { get; set; }
    [Parameter] public DateTime? TimeValuesStartDate { get; set; }
    [Parameter] public EventCallback<DateTime?> TimeValuesStartDateChanged { get; set; }
    [Parameter] public DateTime? TimeValuesEndDate { get; set; }
    [Parameter] public EventCallback<DateTime?> TimeValuesEndDateChanged { get; set; }
    [Parameter] public bool HedgedExposureExist { get; set; } = true;
    [Parameter] public EventCallback<bool> HedgedExposureExistChanged { get; set; }
    [Parameter] public string HedgeType { get; set; } = string.Empty;
    [Parameter] public bool IsShortcut { get; set; }

    // UI State
    [Parameter] public string UserMessage { get; set; } = string.Empty;
    [Parameter] public bool IsError { get; set; }
    [Parameter] public bool IsDeDesignateDisabled { get; set; }

    // Computed properties for field states
    // Per DE-3277, always enabled
    private bool PaymentFieldsEnabled => true;
    
    // Enable/disable start/end dates based on HedgedExposureExist
    private bool DateFieldsEnabled => HedgedExposureExist;
    
    // Cash Payment Type radio buttons are only visible for Termination (reason = 0)
    private bool ShowCashPaymentOptions => DedesignationReason == 0;
    
    // Payment field label changes based on reason
    private string PaymentLabel => DedesignationReason == 0 ? "Termination Fee" : "Payment";
    
    // Show Basis Adjustment Balance only for FairValue, not Shortcut, Termination reason
    private bool ShowBasisAdjustmentFields => HedgeType == "FairValue" && !IsShortcut && DedesignationReason == 0 && ShowBasisAdjustmentBalance;

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
        await DedesignationReasonChanged.InvokeAsync(reason);
        
        // Notify parent to make API call
        if (OnReasonChanged.HasDelegate)
        {
            await OnReasonChanged.InvokeAsync(reason);
        }

        StateHasChanged();
    }

    private async Task HandleCashPaymentTypeChange(int cashPaymentType)
    {
        CashPaymentType = cashPaymentType;
        await CashPaymentTypeChanged.InvokeAsync(cashPaymentType);
        StateHasChanged();
    }

    private async Task HandleHedgedExposureChange(bool exists)
    {
        HedgedExposureExist = exists;
        await HedgedExposureExistChanged.InvokeAsync(exists);
        StateHasChanged();
    }
}
