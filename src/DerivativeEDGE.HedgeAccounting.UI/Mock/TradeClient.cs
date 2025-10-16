namespace DerivativeEDGE.HedgeAccounting.UI.Mock;

public interface ITradeClient
{
    Task<IEnumerable<Trade>> GetTrades();
}

/// <summary>
/// This simulates an Api proxy implementation for demonstration purposes, 
/// use the code generated proxy from the OpenApi.json file instead of creating your own rapper
/// </summary>
public class TradeClient : ITradeClient
{
    public async Task<IEnumerable<Trade>> GetTrades()
    {
        IEnumerable<Trade> trades = default!;
        await Task.Run(() =>
        {
            trades = new[]
            {
                new Trade
                {
                    ClientId = 1,
                    ClientName = "Test",
                    DealDate = "06/05/2023",
                    TransactionDate = new DateTime(2023, 06, 05, 0, 0, 0, DateTimeKind.Utc),
                    TradeDate = new DateTime(2023, 06, 05, 0, 0, 0, DateTimeKind.Utc),
                    TradeStatus = true,
                    Currency = "USD",
                    Notional = 200,
                    Amount = new decimal(-100023455),
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                }
            };
        });
        return trades;
    }
}

/// <summary>
/// This is the TradeClient "Contract"... You don't have to define it. It is to illustrate consuming api client data.
/// </summary>
public class Trade
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
    public string Description { get; set; } = string.Empty;
}
