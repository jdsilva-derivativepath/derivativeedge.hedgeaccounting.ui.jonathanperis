namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentTemplate;

public partial class PreviewTemplateDialog
{
    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Parameter]
    public bool Visible { get; set; } = false;

    [Parameter]
    public EventCallback OnClose { get; set; }

    private string TemplateName { get; set; }

    private string HtmlBody { get; set; }

    private long ClientId { get; set; }

    public async Task FormatContent(long clientId, string templateName, List<DocumentTemplateDetail> documentTemplateDetails)
    {
        TemplateName = templateName;
        ClientId = clientId;

        var contentRequest = new GetDocumentContents.Query(ClientId);
        var contents = await Mediator.Send(contentRequest);

        var hedgeDocumentContents = PrepareContentData(documentTemplateDetails, contents);

        documentTemplateDetails = [.. hedgeDocumentContents
            .Where(x => !x.Hidden && !string.IsNullOrWhiteSpace(x.HtmlBody))
            .OrderBy(x => x.Order)
            .Select(c => c.ToRequestModel())];

        HtmlBody = string.Join("", documentTemplateDetails.Select(detail =>
        {
            var associatedHeader = contents.Data
                .FirstOrDefault(x => x.Id == detail.HedgeDocumentContentId);

            return $"<h3 class='dp-preview-content-header'>{(associatedHeader is null ? detail.Name : associatedHeader.Name)}</h3><div class='dp-preview-content-body'>{detail.HtmlBody}<div/>";
        }));

        StateHasChanged();
    }

    private async Task CloseModal()
    {
        CleanContent();
        await OnClose.InvokeAsync();
    }

    public void CleanContent()
    {
        TemplateName = string.Empty;
        HtmlBody = string.Empty;
    }

    private static List<HedgeDocumentContentViewModel> PrepareContentData(List<DocumentTemplateDetail> documentTemplateDetails, GetDocumentContents.Response contents)
    {
        return [.. contents.Data
        .Select(x =>
        {
            var templateDetail = documentTemplateDetails
                 .FirstOrDefault(cnt => cnt.HedgeDocumentContentId == x.Id);

            if (templateDetail is null)
            {
                return new HedgeDocumentContentViewModel()
                {
                    Id = Guid.NewGuid(),
                    HedgeDocumentContentId = x.Id,
                    Order = x.Order,
                    Hidden = templateDetail is null ? !x.Required : templateDetail.Hidden,
                    HtmlBody = templateDetail?.HtmlBody,
                    Name = x.Name,
                    Required = x.Required
                };
            }
            else
            {
                return new HedgeDocumentContentViewModel()
                {
                    Id = templateDetail.Id,
                    HedgeDocumentContentId = templateDetail.HedgeDocumentContentId,
                    Order = x.Order,
                    Hidden = templateDetail.Hidden,
                    HtmlBody = templateDetail?.HtmlBody,
                    Name = x.Name,
                    Required = x.Required
                };
            }

        })];
    }
}
