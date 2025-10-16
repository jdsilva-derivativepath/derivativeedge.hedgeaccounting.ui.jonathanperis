namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public class GetHrDocument
{
    public sealed record Query(long HrId, Guid TemplateId, bool ShowKeywordValues = true) : IRequest<Response>;
    public sealed record Response(HedgeRelationshipDocumentContent RelationshipDocumentContents);

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var url = $"hedgedocument/hrdocumenttemplate?hrId={request.HrId}&TemplateId={request.TemplateId}&showKeywordValues={request.ShowKeywordValues}";
            Response response = await hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.v1);
            return response;
        }
    }
}
