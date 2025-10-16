namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class DocumentTemplateDetail
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public bool Hidden { get; set; }

    public string HtmlBody { get; set; }

    public int Order { get; set; }

    public Guid HedgeDocumentContentId { get; set; }
}
