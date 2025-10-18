namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetHedgeRelationship
{
    public sealed record Query() : IRequest<Response>;
    public sealed record Response(List<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationships);

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all hedge relationships from the HA API
            var apiHedgeRelationships = await hedgeAccountingApiClient.HedgeRelationshipAllAsync(cancellationToken);

            // Map to UI view models using AutoMapper
            var data = mapper.Map<List<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM>>(apiHedgeRelationships);

            return new Response(data);
        }
    }
}
