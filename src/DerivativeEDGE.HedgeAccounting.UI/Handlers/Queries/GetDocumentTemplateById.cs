namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public class GetDocumentTemplateById
{
    public sealed record Query(Guid Id) : IRequest<Response>;
    public sealed class Response
    {
        public DocumentTemplate DocumentTemplate { get; set; }
    }

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = await _hedgeAccountingApiService.GetAsync<Query, Response>
                ("hedgedocument/template", request, cancellationToken);
            return response;
        }
    }
}
