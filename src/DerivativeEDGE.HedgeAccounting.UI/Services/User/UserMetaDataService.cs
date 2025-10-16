using EdgeRole = DerivativeEDGE.Authorization.AuthClaims.EdgeRole;

namespace DerivativeEDGE.HedgeAccounting.UI.Services.User;

public sealed class UserMetaDataService : IUserMetaDataService
{
    private readonly AuthenticationStateProvider _authState;
    private UserMetaData _userMetaData;
    private bool _isUserMetaDataInitialized;

    public UserMetaDataService(AuthenticationStateProvider authState)
    {
        _authState = authState;
        _userMetaData = new UserMetaData();
    }

    private async Task LoadUserMetaData()
    {
        var state = await _authState.GetAuthenticationStateAsync();
        var clientIdString = state.User.Claims.FirstOrDefault(c => c.Type.Contains("clientId"))?.Value ?? "0";
        var userId = state.User.Claims.FirstOrDefault(c => c.Type.Contains("edgeuserid"))?.Value ?? "0";
        _userMetaData.FirstName = state.User.Claims.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value ?? string.Empty;
        _userMetaData.LastName = state.User.Claims.FirstOrDefault(c => c.Type.Contains("surname"))?.Value ?? string.Empty;
        _userMetaData.EmailAddress = state.User.Claims
            .FirstOrDefault(c => string.Equals(c.Type, "name", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        _userMetaData.ClientId = long.Parse(clientIdString);
        _userMetaData.FullName = $"{_userMetaData.FirstName} {_userMetaData.LastName}";
        _userMetaData.UserId = long.Parse(userId);

        //TODO: we should not be referencing app_metadata for anything
        var metadata = state.User.Claims.FirstOrDefault(c => c.Type.Contains("app_metadata"))?.Value ?? string.Empty;
        var rolesClaim = JsonConvert.DeserializeObject<RolesClaim>(metadata);
        if (rolesClaim != null)
        {
            _userMetaData.Roles = [.. rolesClaim.UserRoles.Select(id => (EdgeRole)id)];
        }
        else
        {
            var jwtRolesClaims = state.User.Claims.Where(c => c.Type.Contains("roles"));
            if (!jwtRolesClaims.Any())
            {
                //Temporary solution to get the roles from the user claims. 
                jwtRolesClaims = state.User.Claims.Where(c => c.Properties.Any(p => p.Value == "roles"));
            }
            _userMetaData.Roles = [.. jwtRolesClaims.Select(j => (EdgeRole)int.Parse(j.Value))];
        }
    }

    public UserMetaData GetUserMetaData()
    {
        if (!_isUserMetaDataInitialized)
        {
            _isUserMetaDataInitialized = true;
            _userMetaData = new UserMetaData();
            Task.Run(LoadUserMetaData).GetAwaiter().GetResult();
        }
        return _userMetaData;
    }

    public bool HasRole(EdgeRole role)
    {
        return GetUserMetaData().Roles.Contains(role);
    }

    public bool IsDpiUser => GetUserMetaData().ClientId == KnownClientIds.DerivativePathIncId;

    public bool HasAllRoles(params EdgeRole[] roles)
    {
        return Array.TrueForAll(roles, GetUserMetaData().Roles.Contains);
    }

    public bool HasAnyRoles(params EdgeRole[] roles)
    {
        return Array.Exists(roles, GetUserMetaData().Roles.Contains);
    }

    private sealed class RolesClaim
    {
        public int[] UserRoles { get; set; } = Array.Empty<int>();
    }
}
