namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public sealed class ClientTrade
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public string DealDate { get; set; } = "";
    public DateTimeOffset TransactionDate { get; set; }
    public DateTime TradeDate { get; set; }
    public bool TradeStatus { get; set; }
    public string Currency { get; set; } = "";
    public decimal Notional { get; set; }
    public decimal Amount { get; set; }
    private decimal ChangePercent
    {
        get
        {
            var previousAmount = Amount - Notional;
            return Math.Round(100 * Notional / previousAmount, 2, MidpointRounding.AwayFromZero);
        }
    }

    public string ChangeDisplay
    {
        get
        {
            var sign = Notional < 0 ? "-" : "+";
            return $"{sign}{Notional} - ({sign}{ChangePercent}%)";
        }
    }

    public string ChangeCssClass => Notional < 0 ? "change-negative" : "change-positive";
    public string Description { get; set; } = string.Empty;
}
