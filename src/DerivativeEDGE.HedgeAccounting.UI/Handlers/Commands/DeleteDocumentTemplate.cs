namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public sealed class DeleteDocumentTemplate
{
    public sealed record Command(Guid Id) : IRequest<Response>;
    public sealed record Response(bool Success);

    public sealed class Handler(IHedgeAccountingApiService hedgeAccountingApiService) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var response = await hedgeAccountingApiService.DeleteAsync<Command, Response>
                ("hedgedocument/templates", request, cancellationToken);
            return response;
        }
    }
}
