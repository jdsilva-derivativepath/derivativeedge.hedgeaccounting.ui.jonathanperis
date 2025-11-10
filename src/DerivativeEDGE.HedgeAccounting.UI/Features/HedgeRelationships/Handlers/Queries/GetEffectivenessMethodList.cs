namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetEffectivenessMethodList
{
    public sealed record Query : IRequest<Response>;
    public sealed record Response(List<DerivativeEDGEHAEntityEffectivenessMethod> EffectivenessMethods);

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<GetEffectivenessMethodList.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Call generated API client to get all effectiveness methods
            var apiEffectivenessMethods = await hedgeAccountingApiClient.GetactiveAsync(cancellationToken);

            var data = apiEffectivenessMethods?.ToList() ?? [];

            if (data == null || data.Count == 0)
            {
                logger.LogWarning("No effectiveness methods returned from API.");
                data = [];
            }

            logger.LogInformation("Retrieved {Count} effectiveness methods", data.Count);
            return new Response(data);
        }
    }
}
