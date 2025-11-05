using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class RunRegression
{
    public sealed record Command(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship, DerivativeEDGEHAEntityEnumHedgeResultType HedgeResultType = DerivativeEDGEHAEntityEnumHedgeResultType.User, DateTime? CurveDate = null) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        public List<string> ValidationErrors { get; set; } = [];
        
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null, List<string> validationErrors = null)
        {
            HasError = hasError;
            ErrorMessage = message;
            Data = data;
            ValidationErrors = validationErrors ?? [];
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, IMapper mapper, ILogger<RunRegression.Handler> logger, TokenProvider tokenProvider) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to run regression for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);

                // Map the full HedgeRelationshipVM to HedgeRelationship entity (following UpdateHedgeRelationship pattern)
                // The API needs the complete hedge relationship data including hedged items, hedging items, effectiveness methods, etc.
                var body = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
                
                // Set required date fields - use CurveDate if provided, otherwise use today's date (matching legacy behavior)
                // Legacy: Model.ValueDate was set to today's date by default and could be changed by user in UI
                var now = DateTimeOffset.Now;
                var valueDate = request.CurveDate.HasValue ? new DateTimeOffset(request.CurveDate.Value) : now;
                body.ValueDate = valueDate;
                body.TimeValuesStartDate = now;
                body.TimeValuesEndDate = now;
                body.TimeValuesFrontRollDate = now;
                body.TimeValuesBackRollDate = now;

                var apiResponse = await hedgeAccountingApiClient.RegressAsync(request.HedgeResultType, body, cancellationToken);

                logger.LogInformation("Successfully completed regression analysis for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);
                return new Response(false, "Regression analysis completed successfully", apiResponse);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error while running regression analysis for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationship.ID, apiEx.StatusCode, apiEx.Response);
                
                // Return the actual API error message to the user
                var errorMessage = !string.IsNullOrEmpty(apiEx.Response) 
                    ? $"Failed to run regression analysis: {apiEx.Response}" 
                    : $"Failed to run regression analysis. Status code: {apiEx.StatusCode}";
                
                return new Response(true, errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while running regression analysis for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);
                return new Response(true, $"Failed to run regression analysis: {ex.Message}");
            }
        }
    }
}
