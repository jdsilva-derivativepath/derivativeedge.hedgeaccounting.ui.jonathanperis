namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public sealed class DeleteHRCachedData
{
    public sealed record Command(long HRId) : IRequest<Response>;

    public sealed class Response
    {
        public string Message { get; set; }
    }

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Command, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var response = await _hedgeAccountingApiService.DeleteAsync<Command, Response>
                ($"hedgedocument/cacheddata?hrId={request.HRId}", request, cancellationToken);
            return response;
        }
    }
}
