namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public record DocumentContentActionResult(string Action, bool Success)
{
    public ErrorMessage ErrorMessage { get; set; }
}
