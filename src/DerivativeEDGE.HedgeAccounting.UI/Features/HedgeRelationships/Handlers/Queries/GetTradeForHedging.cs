namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetTradesForHedging
{
    public sealed record Query(long ItemID, long ClientId) : IRequest<Response>;

    public sealed record Response(DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM HedgeItem);

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<GetTradesForHedging.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Log using API client metadata instead of hard-coded relative path
            var baseUrl = hedgeAccountingApiClient.GetType()
                .GetProperty("BaseUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                .GetValue(hedgeAccountingApiClient) as string ?? string.Empty;
            logger.LogInformation(
                "Invoking {Method} on HedgeAccountingApiClient (BaseUrl: {BaseUrl}) for TradeId {TradeId} ClientId {ClientId}",
                nameof(IHedgeAccountingApiClient.GetForHedgingAsync), baseUrl, request.ItemID, request.ClientId);

            try
            {
                var apiItem = await hedgeAccountingApiClient.GetForHedgingAsync(request.ItemID, request.ClientId, cancellationToken);

                if (apiItem == null)
                {
                    const string error = "Failed to deserialize HedgeRelationshipItem from API response.";
                    logger.LogError(error);
                    throw new InvalidOperationException(error);
                }

                logger.LogInformation("Successfully retrieved HedgeRelationshipItem with ItemID {ItemID}", request.ItemID);
                return new Response(apiItem);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving HedgeRelationshipItem with ItemID {ItemID}", request.ItemID);
                throw;
            }
        }
    }
}
