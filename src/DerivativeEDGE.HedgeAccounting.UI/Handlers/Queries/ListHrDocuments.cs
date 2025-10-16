namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class ListHrDocuments
{
    public sealed record Query(long HedgeRelationshipId) : IRequest<Response>;
    public sealed record Response(List<HedgeRelationshipDocumentContent> RelationshipDocumentContents);

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var url = $"hedgedocument/hrtemplates?hedgeRelationshipId={request.HedgeRelationshipId}";
            Response response = await hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.v1);
            return response ?? new Response([]);
        }
    }
}
