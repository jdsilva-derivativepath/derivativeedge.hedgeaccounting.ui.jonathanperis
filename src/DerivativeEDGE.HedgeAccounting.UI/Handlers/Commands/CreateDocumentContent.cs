namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public sealed class CreateDocumentContent
{
    public sealed record Command(string Name, long ClientId, long UserId, bool Required) : IRequest<Response>;

    public sealed class Response
    {
        public string Message { get; set; }
    }

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Command, Response>
    {
        private readonly IHedgeAccountingApiService _hedgeAccountingApiService = hedgeAccountingApiService;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var response = await _hedgeAccountingApiService.PostAsync<Command, Response>
                ("hedgedocument/contents", request, cancellationToken);
            return response;
        }
    }
}
