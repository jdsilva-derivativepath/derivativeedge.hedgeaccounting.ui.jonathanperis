namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class HedgeDocumentContentViewModel
{
    public Guid Id { get; set; }

    public Guid HedgeDocumentContentId { get; set; }

    public string Name { get; set; }

    public string HtmlBody { get; set; }

    public bool Required { get; set; }

    public bool Hidden { get; set; }

    public int Order { get; set; }

    public DocumentTemplateDetail ToRequestModel()
    {
        return new DocumentTemplateDetail
        {
            Id = Id,
            Name = Name,
            Hidden = Hidden,
            HtmlBody = HtmlBody,
            Order = Order,
            HedgeDocumentContentId = HedgeDocumentContentId,
        };
    }
}
