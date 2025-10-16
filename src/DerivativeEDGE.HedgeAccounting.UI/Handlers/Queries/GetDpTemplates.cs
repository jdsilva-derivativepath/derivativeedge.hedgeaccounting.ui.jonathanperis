namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetDpTemplates
{
    public class Query : IRequest<Response>
    {

    }

    public class Response
    {
        public List<DocumentTemplate> DocumentTemplates { get; set; }
    }

    public class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var url = $"hedgedocument/templates?clientId=1&enabledOnly=true";
            Response response = await _hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.v1);
            return response ?? new Response();
        }
    }
}
