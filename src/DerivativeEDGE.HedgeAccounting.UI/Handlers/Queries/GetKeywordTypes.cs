namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetKeywordTypes
{
    public class Query : IRequest<List<EnumType>>
    {

    }

    public class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Query, List<EnumType>>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;
        public async Task<List<EnumType>> Handle(Query request, CancellationToken cancellationToken)
        {
            var url = $"hedgedocument/keywords";
            List<EnumType> response = await _hedgeAccountingApiService.GetAsync<List<EnumType>>(url, HedgeAccountingApiVersions.v1);
            return response;
        }
    }
}
