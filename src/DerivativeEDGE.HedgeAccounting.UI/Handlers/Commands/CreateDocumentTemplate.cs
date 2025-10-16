namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public class CreateDocumentTemplate
{
    public sealed record Command(long ClientId, string Name, string Description,
        bool Enabled, long CreatedById,
        List<DocumentTemplateDetail> HedgeDocumentTemplateDetails) : IRequest<Response>;

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
                ("hedgedocument/templates", request, cancellationToken);
            return response;
        }
    }
}
