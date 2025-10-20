using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class RedraftHedgeRelationship
{
    public sealed record Command(long HedgeRelationshipId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null)
        {
            HasError = hasError;
            ErrorMessage = message;
            Data = data;
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler(
        IHedgeAccountingApiClient hedgeAccountingApiClient,
        IMapper mapper,
        ILogger<RedraftHedgeRelationship.Handler> logger,
        TokenProvider tokenProvider) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to redraft hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                // First, get the current hedge relationship
                var currentHedgeRelationship = await hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId, 
                    cancellationToken);

                // Validate redraft requirements
                var validationErrors = RedraftValidator.Validate(currentHedgeRelationship);
                if (validationErrors.Count != 0)
                {
                    var errorMessage = string.Join("; ", validationErrors);
                    logger.LogWarning("Redraft validation failed for hedge relationship ID: {HedgeRelationshipId}. Errors: {Errors}", 
                        request.HedgeRelationshipId, errorMessage);
                    return new Response(true, errorMessage);
                }

                // Map to entity for API call
                var hedgeRelationshipEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(currentHedgeRelationship);

                // Call the Redraft API endpoint
                var redraftedHedgeRelationship = await hedgeAccountingApiClient.RedraftAsync(
                    hedgeRelationshipEntity, 
                    cancellationToken);

                logger.LogInformation("Successfully redrafted hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully redrafted", redraftedHedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error occurred while redrafting hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                
                // Return the actual API error message to the user
                var errorMessage = !string.IsNullOrEmpty(apiEx.Response) 
                    ? $"Failed to redraft hedge relationship: {apiEx.Response}" 
                    : $"Failed to redraft hedge relationship. Status code: {apiEx.StatusCode}";
                
                return new Response(true, errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while redrafting hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, $"Failed to redraft hedge relationship: {ex.Message}");
            }
        }
    }
}
