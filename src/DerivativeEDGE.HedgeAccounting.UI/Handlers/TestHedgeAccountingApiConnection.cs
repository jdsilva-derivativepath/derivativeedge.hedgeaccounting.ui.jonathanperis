namespace DerivativeEDGE.HedgeAccounting.UI.Handlers;

public class TestHedgeAccountingApiConnection
{
    public sealed class Response
    {
        public string ResponseData { get; set; }
        public Response(string responseData)
        {
            ResponseData = responseData;
        }
    }

    public sealed record Request(string RequestData) : IRequest<Response>
    {
    }

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService;
        public Handler(IHedgeAccountingApiService hedgeAccountingApiService)
        {
            _hedgeAccountingApiService = hedgeAccountingApiService;
        }
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var url = $"initial?requestData={request.RequestData}";
            Response response = await _hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.None);
            return response ?? new Response(string.Empty);
        }
    }
}
