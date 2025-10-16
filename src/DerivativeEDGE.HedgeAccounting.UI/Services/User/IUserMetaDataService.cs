using EdgeRole = DerivativeEDGE.Authorization.AuthClaims.EdgeRole;

namespace DerivativeEDGE.HedgeAccounting.UI.Services.User;

public interface IUserMetaDataService
{
    bool HasAllRoles(params EdgeRole[] roles);
    bool HasAnyRoles(params EdgeRole[] roles);
    bool HasRole(EdgeRole role);
    bool IsDpiUser { get; }
    UserMetaData GetUserMetaData();
}
