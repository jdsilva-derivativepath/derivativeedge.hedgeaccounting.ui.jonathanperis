namespace DerivativeEDGE.HedgeAccounting.UI.Handlers;

public class TestHedgeAccountingApiConnection
{
    public sealed class Response(string responseData)
    {
        public string ResponseData { get; set; } = responseData;
    }

    public sealed record Request(string RequestData) : IRequest<Response>
    {
    }

    public class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var url = $"initial?requestData={request.RequestData}";
            Response response = await hedgeAccountingApiService.GetAsync<Response>(url, HedgeAccountingApiVersions.None);
            return response ?? new Response(string.Empty);
        }
    }
}
