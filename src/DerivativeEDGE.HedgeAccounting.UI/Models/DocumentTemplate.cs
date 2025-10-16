namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class DocumentTemplate
{
    public Guid Id { get; set; }

    public long ClientId { get; set; }
    public string Name { get; set; }

    public string Description { get; set; }

    public List<DocumentTemplateDetail> HedgeDocumentTemplateDetails { get; set; } = [];

    public bool Enabled { get; set; }
    public long CreatedById { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset CreatedOn { get; set; }

    public long ModifiedById { get; set; }
    public string ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedOn { get; set; }
}
