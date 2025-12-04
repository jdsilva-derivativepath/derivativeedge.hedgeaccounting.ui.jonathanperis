namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetHedgeRelationshipCurrencyList
{
    public sealed record Query : IRequest<Response>;
    public sealed record Response(List<HedgeCurrencyDropdownItem> Currency);

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<GetHedgeRelationshipCurrencyList.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Call generated API client
            var apiCurrencies = await hedgeAccountingApiClient.CurrencyAllAsync(cancellationToken);

            // Map to VM (manual lightweight mapping)
            var data = apiCurrencies?
                .Select(c => new HedgeCurrencyDropdownItem
                {
                    LongName = c.LongName ?? c.ShortName ?? c.Label ?? string.Empty,
                    ShortName = c.ShortName ?? c.Label ?? string.Empty
                })
                .Where(c => !string.IsNullOrWhiteSpace(c.ShortName))
                .ToList();

            if (data == null)
            {
                logger.LogError("Failed to deserialize currency data.");
                data = [];
            }

            logger.LogInformation("Retrieved {Count} currencies", data.Count);
            return new Response(data);
        }
    }
}
