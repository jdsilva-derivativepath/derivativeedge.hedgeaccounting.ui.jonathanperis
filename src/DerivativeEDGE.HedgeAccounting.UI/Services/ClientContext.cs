namespace DerivativeEDGE.HedgeAccounting.UI.Services;

internal record ClientContext : IClientContext
{
    public string ClientId { get; set; } = string.Empty;
    // We can pass additional criteria to validate a feature flag.
    public Dictionary<string, object> Attributes { get; set; } = default!;
}