using ApiException = DerivativeEDGE.Identity.API.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class FindDocumentTemplate
{
    public sealed record Query(long HedgeRelationshipId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public bool HasTemplate { get; set; }
        
        public Response(bool hasError, string message, bool hasTemplate = false)
        {
            HasError = hasError;
            ErrorMessage = message;
            HasTemplate = hasTemplate;
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler(
        IHedgeAccountingApiClient hedgeAccountingApiClient,
        ILogger<FindDocumentTemplate.Handler> logger,
        TokenProvider tokenProvider) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Checking for document template for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);

                // Call API to check if document template exists
                var hasTemplate = await hedgeAccountingApiClient.FindDocumentTemplateAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                logger.LogInformation("Document template check completed for hedge relationship ID: {HedgeRelationshipId}, HasTemplate: {HasTemplate}", 
                    request.HedgeRelationshipId, hasTemplate);
                    
                return new Response(false, "Document template check completed", hasTemplate);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error occurred while checking document template for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to check document template: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking document template for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                return new Response(true, "Failed to check document template due to an unexpected error");
            }
        }
    }
}
