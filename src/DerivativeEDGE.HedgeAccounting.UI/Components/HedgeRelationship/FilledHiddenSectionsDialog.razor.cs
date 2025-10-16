namespace DerivativeEDGE.HedgeAccounting.UI.Components.HedgeRelationship;

public partial class FilledHiddenSectionsDialog
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    public List<string> FilledContents { get; set; }

    private async Task CloseButtonClicked()
    {
        await OnClose.InvokeAsync();
    }
}
