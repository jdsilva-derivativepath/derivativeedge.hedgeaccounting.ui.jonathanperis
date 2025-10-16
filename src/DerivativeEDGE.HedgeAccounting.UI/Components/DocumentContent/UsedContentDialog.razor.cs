namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentContent;

public partial class UsedContentDialog
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback OnViewTemplates { get; set; }

    [Parameter]
    public EventCallback<Models.DocumentContent> OnSave { get; set; }

    public string ContentName { get; set; }

    public List<string> TemplateNames { get; set; }

    private async Task CloseButtonClicked()
    {
        await OnClose.InvokeAsync();
    }

    private async Task ViewTemplatesButtonClicked()
    {
        await OnViewTemplates.InvokeAsync();
    }
}
