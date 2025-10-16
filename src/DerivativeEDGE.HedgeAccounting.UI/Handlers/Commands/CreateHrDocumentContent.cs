namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public class CreateHrDocumentContent
{
    public sealed record Command(long HedgeRelationshipId, string Name, string Description,
        long CreatedById, Guid? HedgeDocumentTemplateId,
        List<DocumentTemplateDetail> HedgeRelationshipDocumentContentDetails) : IRequest<Response>;

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
                ("hedgedocument/hrdocumentcontent", request, cancellationToken);
            return response;
        }
    }
}
