namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public record HrDocumentActionResult(string Action, bool Success)
{
    public ErrorMessage ErrorMessage { get; set; }
}
