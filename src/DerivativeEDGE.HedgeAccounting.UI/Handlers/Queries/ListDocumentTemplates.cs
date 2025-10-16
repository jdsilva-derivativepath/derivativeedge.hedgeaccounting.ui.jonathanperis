namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class ListDocumentTemplates
{
    public sealed record Query(long ClientId) : IRequest<Response>;
    public sealed record Response(List<DocumentTemplate> DocumentTemplates);

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var url = $"hedgedocument/templates?clientId={request.ClientId}&enabledOnly=false";
            Response response = await hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.v1);
            return response ?? new Response([]);
        }
    }
}
