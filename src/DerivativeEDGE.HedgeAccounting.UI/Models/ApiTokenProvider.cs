namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class ApiTokenProvider
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.Now;
    public string TokenType { get; set; } = string.Empty;
}
