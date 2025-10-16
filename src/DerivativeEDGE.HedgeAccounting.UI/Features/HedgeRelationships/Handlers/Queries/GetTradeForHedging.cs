using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetTradesForHedging
{
    public sealed record Query(long ItemID, long ClientId) : IRequest<Response>;

    public sealed record Response(DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM HedgeItem);

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<Handler> logger, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Log using API client metadata instead of hard-coded relative path
            var baseUrl = _hedgeAccountingApiClient.GetType()
                .GetProperty("BaseUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                .GetValue(_hedgeAccountingApiClient) as string ?? string.Empty;
            _logger.LogInformation(
                "Invoking {Method} on HedgeAccountingApiClient (BaseUrl: {BaseUrl}) for TradeId {TradeId} ClientId {ClientId}",
                nameof(IHedgeAccountingApiClient.GetForHedgingAsync), baseUrl, request.ItemID, request.ClientId);

            try
            {
                var apiItem = await _hedgeAccountingApiClient.GetForHedgingAsync(request.ItemID, request.ClientId, cancellationToken);

                if (apiItem == null)
                {
                    const string error = "Failed to deserialize HedgeRelationshipItem from API response.";
                    _logger.LogError(error);
                    throw new InvalidOperationException(error);
                }

                _logger.LogInformation("Successfully retrieved HedgeRelationshipItem with ItemID {ItemID}", request.ItemID);
                return new Response(apiItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving HedgeRelationshipItem with ItemID {ItemID}", request.ItemID);
                throw;
            }
        }
    }
}
