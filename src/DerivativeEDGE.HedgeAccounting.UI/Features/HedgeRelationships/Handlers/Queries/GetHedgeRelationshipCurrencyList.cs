namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetHedgeRelationshipCurrencyList
{
    public sealed record Query : IRequest<Response>;
    public sealed record Response(List<HedgeCurrencyDropdownItem> Currency);

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly ILogger<Handler> _logger;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<Handler> logger)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Call generated API client
            var apiCurrencies = await _hedgeAccountingApiClient.CurrencyAllAsync(cancellationToken);

            // Map to VM (manual lightweight mapping)
            var data = apiCurrencies?
                .Select(c => new HedgeCurrencyDropdownItem
                {
                    LongName = c.LongName ?? c.ShortName ?? c.Label ?? string.Empty,
                    ShortName = c.ShortName ?? c.Label ?? string.Empty
                })
                .Where(c => !string.IsNullOrWhiteSpace(c.ShortName))
                .OrderBy(c => c.ShortName)
                .ToList();

            if (data == null)
            {
                _logger.LogError("Failed to deserialize currency data.");
                data = [];
            }

            _logger.LogInformation("Retrieved {Count} currencies", data.Count);
            return new Response(data);
        }
    }
}
