namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Commands;

public class UpdateHrDocumentContent
{
    public sealed record Command(long HedgeRelationshipId, string Name, string Description,
        long ModifiedById, Guid? HedgeDocumentTemplateId, bool IsChangingTemplate,
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
            var response = await _hedgeAccountingApiService.PatchAsync<Command, Response>
                ("hedgedocument/hrdocumentcontent", request, cancellationToken);
            return response;
        }
    }
}
