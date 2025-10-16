namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class HedgeRelationshipDocumentKeywordValues
{
    public Guid Id { get; set; }

    public long HRId { get; set; }

    public Dictionary<string, string> KeywordValues { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    public long CreatedById { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public long ModifiedById { get; set; }

    public DateTimeOffset? ModifiedOn { get; set; }
}
