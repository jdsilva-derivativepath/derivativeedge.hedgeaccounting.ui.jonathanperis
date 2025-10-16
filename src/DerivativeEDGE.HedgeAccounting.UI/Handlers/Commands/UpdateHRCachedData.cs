namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public sealed class UpdateHRCachedData
{
    public sealed record Command(long HRId, string HRPayload, string TemplatePayload, string Source, long UserId) : IRequest<Response>;

    public sealed class Response
    {
        public string Message { get; set; }
    }

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Command, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var response = await _hedgeAccountingApiService.PatchAsync<Command, Response>
                ("hedgedocument/cacheddata", request, cancellationToken);
            return response;
        }
    }
}
