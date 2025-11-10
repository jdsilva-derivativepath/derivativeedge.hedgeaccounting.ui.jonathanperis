namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class CheckDocumentTemplateKeywords
{
    public sealed record Query(long HedgeRelationshipId) : IRequest<Response>;
    
    public sealed record Response(bool HasEmptyKeyword);

    public sealed class Handler(
        IHedgeAccountingApiClient hedgeAccountingApiClient,
        ILogger<CheckDocumentTemplateKeywords.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Checking document template keywords for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);

                var result = await hedgeAccountingApiClient.CheckDocumentTemplateKeywordsAsync(
                    request.HedgeRelationshipId, 
                    cancellationToken);

                logger.LogInformation("Document template keywords check completed for HedgeRelationship ID: {HedgeRelationshipId}, HasEmptyKeyword: {HasEmptyKeyword}", 
                    request.HedgeRelationshipId, result.HasEmptyKeyword);

                return new Response(result.HasEmptyKeyword);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to check document template keywords for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                throw;
            }
        }
    }
}
