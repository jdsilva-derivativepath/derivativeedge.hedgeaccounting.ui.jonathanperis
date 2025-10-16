namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public sealed class UpdateDocumentContent
{
    public sealed record Command(Guid Id, string Name, long ClientId, bool Required) : IRequest<Response>;

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
                ("hedgedocument/contents", request, cancellationToken);
            return response;
        }
    }
}
