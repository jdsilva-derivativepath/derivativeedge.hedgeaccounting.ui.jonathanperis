namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class SaveHedgeRelationship
{
    public sealed record Command(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null)
        {
            HasError = hasError;
            Message = message;
            Data = data;
        }
        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;

    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<SaveHedgeRelationship.Handler> logger, IMapper mapper) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to save hedge relationship.");

                // Apply field cleanup and defaults before saving (matches legacy submit logic)
                SaveHedgeRelationshipValidator.ApplyFieldCleanupAndDefaults(request.HedgeRelationship);

                // Map DTO to API entity
                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Set required date fields to today's date (matching legacy behavior)
                var now = DateTimeOffset.Now;
                apiEntity.ValueDate = now;
                apiEntity.TimeValuesStartDate = now;
                apiEntity.TimeValuesEndDate = now;
                apiEntity.TimeValuesFrontRollDate = now;
                apiEntity.TimeValuesBackRollDate = now;
                apiEntity.CreatedOn = now;
                apiEntity.ModifiedOn = now;
                DerivativeEDGEHAApiViewModelsHedgeRelationshipVM createdVm = null;

                try
                {
                    createdVm = await hedgeAccountingApiClient.HedgeRelationshipPOSTAsync(apiEntity, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    // Mimic original warning log signature
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message; // no direct ReasonPhrase in generated exception
                    var content = ex.GetType().GetProperty("Response")?.GetValue(ex, null);
                    logger.LogWarning("Failed to create hedge relationship. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}", statusCode, reason, content);
                    return new Response(true, "Failed to create hedge relationship");
                }

                logger.LogInformation("Successfully saved hedge relationship.");
                return new Response(false, "Successfully saved hedge relationship", createdVm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while saving hedge relationship.");
                return new Response(true, "Failed to save hedge relationship");
            }
        }
    }
}