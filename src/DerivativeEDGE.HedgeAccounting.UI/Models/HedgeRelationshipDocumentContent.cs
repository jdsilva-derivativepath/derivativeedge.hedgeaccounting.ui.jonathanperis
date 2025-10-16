namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class HedgeRelationshipDocumentContent
{
    public Guid Id { get; set; }

    public long HRId { get; set; }
    public bool HRDeDesignated { get; set; }
    public Guid? HedgeDocumentTemplateId { get; set; }
    public string DocumentName { get; set; }
    public string DocumentDescription { get; set; }
    public List<HedgeRelationshipDocumentContentDetail> HedgeRelationshipDocumentContents { get; set; } = [];

    public long CreatedById { get; set; }
    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public long ModifiedById { get; set; }
    public string ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedOn { get; set; }
}
