namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class ReDesignateDialog
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnReDesignated { get; set; }

    // Model properties
    [Parameter] public DateTime? RedesignationDate { get; set; }
    [Parameter] public DateTime? TimeValuesStartDate { get; set; }
    [Parameter] public DateTime? TimeValuesEndDate { get; set; }
    [Parameter] public decimal? Payment { get; set; } = 0;
    [Parameter] public string PaymentFrequency { get; set; } = string.Empty;
    [Parameter] public string DayCountConv { get; set; } = string.Empty;
    [Parameter] public string PayBusDayConv { get; set; } = string.Empty;
    [Parameter] public bool AdjustedDates { get; set; }
    [Parameter] public bool MarkAsAcquisition { get; set; }
    [Parameter] public bool IsDocTemplateFound { get; set; }

    // Data sources
    [Parameter] public List<PaymentFrequencyOption> AvailablePaymentFrequencies { get; set; } = new();
    [Parameter] public List<DayCountConvOption> AvailableDayCountConventions { get; set; } = new();
    [Parameter] public List<PayBusDayConvOption> AvailablePayBusDayConventions { get; set; } = new();

    private string ValidationMessage { get; set; } = string.Empty;

    // Validation logic from legacy: isRedesignationValid()
    private bool IsValid
    {
        get
        {
            // Payment must be non-zero
            if (Payment == null || Payment == 0)
                return false;

            // Redesignation date must be valid
            if (RedesignationDate == null)
                return false;

            // Start and End dates must be valid
            if (TimeValuesStartDate == null || TimeValuesEndDate == null)
                return false;

            // Start date must be before End date
            if (TimeValuesStartDate >= TimeValuesEndDate)
                return false;

            // Required fields must be filled
            if (string.IsNullOrEmpty(PayBusDayConv) ||
                string.IsNullOrEmpty(PaymentFrequency) ||
                string.IsNullOrEmpty(DayCountConv))
                return false;

            return true;
        }
    }

    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private async Task HandleReDesignate()
    {
        if (!IsValid)
        {
            ValidationMessage = "Please fill in all required fields and ensure dates are valid.";
            StateHasChanged();
            return;
        }

        if (OnReDesignated.HasDelegate)
        {
            await OnReDesignated.InvokeAsync();
        }
        await HandleClose();
    }
}
