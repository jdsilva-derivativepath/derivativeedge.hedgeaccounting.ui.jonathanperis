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

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly TokenProvider _tokenProvider;

        public Handler(
            IHedgeAccountingApiClient hedgeAccountingApiClient,
            ILogger<Handler> logger,
            TokenProvider tokenProvider)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _logger = logger;
            _tokenProvider = tokenProvider;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Checking for document template for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);

                // Call API to check if document template exists
                var hasTemplate = await _hedgeAccountingApiClient.FindDocumentTemplateAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                _logger.LogInformation("Document template check completed for hedge relationship ID: {HedgeRelationshipId}, HasTemplate: {HasTemplate}", 
                    request.HedgeRelationshipId, hasTemplate);
                    
                return new Response(false, "Document template check completed", hasTemplate);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while checking document template for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to check document template: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking document template for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                return new Response(true, "Failed to check document template due to an unexpected error");
            }
        }
    }
}
