namespace DerivativeEDGE.HedgeAccounting.UI.Components.HedgeRelationship;

public partial class PreviewHrDocumentDialog
{
    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public long? HedgeRelationshipId { get; set; }

    [Parameter]
    public bool? IsNewHedgeAccounting { get; set; }

    private string TemplateName { get; set; }

    private string HtmlBody { get; set; }

    private long ClientId { get; set; }

    public bool AllowEditHrDocument { get; set; }

    public void FormatContent(long clientId, string templateName, List<DocumentTemplateDetail> documentTemplateDetails, bool hrDeDesignated)
    {
        AllowEditHrDocument = !hrDeDesignated;

        TemplateName = templateName;
        ClientId = clientId;

        documentTemplateDetails = [.. documentTemplateDetails
            .Where(x => !x.Hidden && !string.IsNullOrWhiteSpace(x.HtmlBody))
            .OrderBy(x => x.Order)];

        HtmlBody = string.Join("", documentTemplateDetails.Select(detail =>
        {
            return $"<h3 class='dp-preview-content-header'>{(detail.Name)}</h3><div class='dp-preview-content-body'>{detail.HtmlBody}<div/>";
        }));

        StateHasChanged();
    }

    private void EditHrDocument()
    {
        NavigationManager.NavigateTo($"edit-hrdocument?HedgeRelationshipId={HedgeRelationshipId}&ClientId={ClientId}&IsNewHedgeAccounting={IsNewHedgeAccounting}");
    }

    private async Task CancelPreview()
    {
        var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId.GetValueOrDefault(), string.Empty, string.Empty, "HAUI", 0);
        await Mediator.Send(updateHRCachedData);

        if (IsNewHedgeAccounting.GetValueOrDefault())
        {
            NavigationManager.NavigateTo($"/hedgeaccountingapp/hedgerelationship?Id={HedgeRelationshipId.GetValueOrDefault()}");
        }
        else
        {
            NavigationManager.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId.GetValueOrDefault()}");
        }
    }
}
