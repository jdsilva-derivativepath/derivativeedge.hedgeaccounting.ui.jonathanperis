namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public class GetHrDocumentKeywordValue
{
    public sealed record Query(long HrId) : IRequest<Response>;
    public sealed record Response(HedgeRelationshipDocumentKeywordValues DocumentKeywordValues);

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var url = $"hedgedocument/hrdocumentkeywordvalue?hrId={request.HrId}";
            Response response = await hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.v1);
            return response;
        }
    }
}
