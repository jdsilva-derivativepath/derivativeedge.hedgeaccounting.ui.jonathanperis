namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class DpConfirmationModal
{
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter] public string Title { get; set; } = "Confirmation";
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public string ConfirmText { get; set; } = "OK";
    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public bool ShowCancelButton { get; set; } = true;
    [Parameter] public bool ShowCloseIcon { get; set; } = true;

    [Parameter] public EventCallback OnConfirmed { get; set; }
    [Parameter] public EventCallback OnCancelled { get; set; }

    [Parameter] public string Width { get; set; } = "30rem";
    [Parameter] public string Height { get; set; } = "14rem";

    private async Task HandleCancel()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
        if (OnCancelled.HasDelegate)
            await OnCancelled.InvokeAsync();
    }

    private async Task HandleConfirm()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
        if (OnConfirmed.HasDelegate)
            await OnConfirmed.InvokeAsync();
    }
}
