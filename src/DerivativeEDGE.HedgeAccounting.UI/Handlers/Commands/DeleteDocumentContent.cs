namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public sealed class DeleteDocumentContent
{
    public sealed record Command(Guid Id) : IRequest<Response>;

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
                ("hedgedocument/contents", request, cancellationToken);
            return response;
        }
    }
}
