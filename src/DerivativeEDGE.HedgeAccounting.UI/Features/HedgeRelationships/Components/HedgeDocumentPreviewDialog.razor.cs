namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class HedgeDocumentPreviewDialog
{
    #region Parameters
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public string HedgeDocumentContent { get; set; } = string.Empty;
    #endregion

    #region Event Handlers
    private async Task HandleClose()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }
    #endregion
}
