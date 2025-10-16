namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class AllowedBehavior
{
    public long ClientId { get; init; }
    public bool IsDpiUser { get; init; }
    public long UserId { get; init; }
    public bool IsSaasSwaSClient { get; set; }
    public bool FullAccess { get; set; }
}
