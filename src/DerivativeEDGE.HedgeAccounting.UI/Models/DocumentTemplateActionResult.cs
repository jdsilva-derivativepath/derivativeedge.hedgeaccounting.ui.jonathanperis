namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public record DocumentTemplateActionResult(string Action, bool Success)
{
    public ErrorMessage ErrorMessage { get; set; }
}
