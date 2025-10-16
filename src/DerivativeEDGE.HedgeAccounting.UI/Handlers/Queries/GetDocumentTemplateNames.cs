namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetDocumentTemplateNames
{
    public sealed record Query(long ClientId, Guid contentId) : IRequest<Response>;

    public sealed class Response
    {
        public List<string> TemplateNames { get; set; } 
    }

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = await _hedgeAccountingApiService.GetAsync<Query, Response>
                ("hedgedocument/templatenames", request, cancellationToken);
            return response;
        }
    }
}
