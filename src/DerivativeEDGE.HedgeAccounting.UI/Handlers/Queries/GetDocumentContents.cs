namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetDocumentContents
{
    public sealed record Query(long ClientId) : IRequest<Response>;

    public sealed class Response
    {
        public List<DocumentContent> Data { get; set; } 
    }

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = await _hedgeAccountingApiService.GetAsync<Query, Response>
                ("hedgedocument/contents", request, cancellationToken);
            if (response != null)
            {
                response.Data ??= [];
            }
            return response;
        }
    }
}
