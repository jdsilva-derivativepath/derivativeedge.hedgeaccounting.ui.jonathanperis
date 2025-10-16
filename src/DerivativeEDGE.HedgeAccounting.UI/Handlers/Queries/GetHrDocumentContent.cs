namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public class GetHrDocumentContent
{
    public sealed record Query(long HedgeRelationshipId, Guid TemplateId) : IRequest<Response>;
    public sealed record Response(HedgeRelationshipDocumentContent RelationshipDocumentContent);

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {

            var response = await hedgeAccountingApiService.GetAsync<Query, Response>
             ("hedgedocument/hrtemplate", request, cancellationToken);

            return response;
        }
    }
}
