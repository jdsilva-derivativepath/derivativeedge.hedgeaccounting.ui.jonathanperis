namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetHedgeRelationship
{
    public sealed record Query() : IRequest<Response>;
    public sealed record Response(List<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationships);

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMapper _mapper;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all hedge relationships from the HA API
            var apiHedgeRelationships = await _hedgeAccountingApiClient.HedgeRelationshipAllAsync(cancellationToken);

            // Map to UI view models using AutoMapper
            var data = _mapper.Map<List<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM>>(apiHedgeRelationships);

            return new Response(data);
        }
    }
}
