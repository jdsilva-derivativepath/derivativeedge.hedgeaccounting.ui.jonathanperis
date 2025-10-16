namespace DerivativeEDGE.HedgeAccounting.UI.Pages;

	public class LoginModel : PageModel
	{

    private sealed class RolesClaim
    {
        public int[] UserRoles { get; set; } = [];
    }
    public async Task OnGet()
    {
        // Local Login is no longer used. /login should be via the reverse proxy back to Edge's login
    }
	}
