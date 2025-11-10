namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetOptionAmortizationDefaults
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAEntityValueObjectsOptionAmortizationDefaultValues Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public Response(bool hasError, string message, DerivativeEDGEHAEntityValueObjectsOptionAmortizationDefaultValues data = null)
        {
            HasError = hasError;
            Message = message;
            Data = data;
        }
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<GetOptionAmortizationDefaults.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
    {
        private const string ErrorMessage = "Failed to load option amortization defaults";

        public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(tokenProvider.AccessToken))
                {
                    logger.LogWarning("Access token is null or empty for HedgeRelationship ID: {HedgeRelationshipId}", query.HedgeRelationship?.ID);
                    return new Response(true, ErrorMessage);
                }

                logger.LogInformation("Fetching option amortization defaults for HedgeRelationship ID: {HedgeRelationshipId}", query.HedgeRelationship?.ID);

                try
                {
                    // Map ViewModel to Entity for API call (legacy: hr_hedgeRelationshipAddEditCtrl.js line 3300)
                    var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(query.HedgeRelationship);
                    
                    var defaults = await hedgeAccountingApiClient.GetOptionAmortizationDefaultsAsync(apiEntity, cancellationToken);
                    if (defaults == null)
                    {
                        logger.LogWarning("Failed to fetch option amortization defaults. StatusCode: {StatusCode}", "(no content)");
                        return new Response(true, ErrorMessage);
                    }

                    logger.LogInformation("Successfully fetched option amortization defaults for HedgeRelationship ID: {HedgeRelationshipId}", query.HedgeRelationship?.ID);
                    return new Response(false, "Success", defaults);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    // Attempt to get StatusCode property via reflection to preserve original log message format
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);

                    // Check for specific "Hedging Item does not exist!" error message
                    if (ex.InnerException?.Message?.Contains("Hedging Item does not exist") == true)
                    {
                        logger.LogWarning("Failed to fetch option amortization defaults - Hedging Item does not exist for HedgeRelationship ID: {HedgeRelationshipId}. StatusCode: {StatusCode}",
                            query.HedgeRelationship?.ID, statusCode);
                        return new Response(true, "The Hedging Item does not exist!");
                    }

                    logger.LogWarning("Failed to fetch option amortization defaults. StatusCode: {StatusCode}", statusCode);
                    return new Response(true, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching option amortization defaults for HedgeRelationship ID: {HedgeRelationshipId}", query.HedgeRelationship?.ID);
                return new Response(true, ErrorMessage);
            }
        }
    }
}
