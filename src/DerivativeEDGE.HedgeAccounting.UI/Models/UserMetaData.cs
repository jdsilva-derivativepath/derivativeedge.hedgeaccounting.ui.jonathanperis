using EdgeRole = DerivativeEDGE.Authorization.AuthClaims.EdgeRole;

namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class UserMetaData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public long ClientId { get; set; }
    public long UserId { get; set; }
    public List<EdgeRole> Roles { get; set; } = [];
}
