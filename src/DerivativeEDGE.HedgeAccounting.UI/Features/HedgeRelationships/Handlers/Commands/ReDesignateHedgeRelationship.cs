using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class ReDesignateHedgeRelationship
{
    public sealed record Command(
        long HedgeRelationshipId,
        DateTime RedesignationDate,
        decimal Payment,
        DateTime TimeValuesStartDate,
        DateTime TimeValuesEndDate,
        string PaymentFrequency,
        string DayCountConv,
        string PayBusDayConv,
        bool AdjustedDates,
        bool MarkAsAcquisition) : IRequest<Response>;

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
        ILogger<ReDesignateHedgeRelationship.Handler> logger,
        TokenProvider tokenProvider) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to redesignate hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                // Get the current hedge relationship
                var currentHedgeRelationship = await hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                // Validate redesignation requirements
                var validationErrors = ReDesignateValidator.Validate(
                    currentHedgeRelationship, 
                    request.RedesignationDate);
                    
                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("; ", validationErrors);
                    logger.LogWarning("Re-designation validation failed for hedge relationship ID: {HedgeRelationshipId}. Errors: {Errors}", 
                        request.HedgeRelationshipId, errorMessage);
                    return new Response(true, errorMessage);
                }

                // Map to entity and update redesignation properties
                var hedgeRelationshipEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(currentHedgeRelationship);
                
                // Set redesignation specific properties
                hedgeRelationshipEntity.RedesignationDate = new DateTimeOffset(request.RedesignationDate);
                hedgeRelationshipEntity.Payment = (double)request.Payment;
                hedgeRelationshipEntity.TimeValuesStartDate = new DateTimeOffset(request.TimeValuesStartDate);
                hedgeRelationshipEntity.TimeValuesEndDate = new DateTimeOffset(request.TimeValuesEndDate);
                
                // Parse enum values
                if (Enum.TryParse<DerivativeEDGEDomainEntitiesEnumsPaymentFrequency>(request.PaymentFrequency, out var paymentFreq))
                {
                    hedgeRelationshipEntity.PaymentFrequency = paymentFreq;
                }
                
                if (Enum.TryParse<DerivativeEDGEDomainEntitiesEnumsDayCountConv>(request.DayCountConv, out var dayCount))
                {
                    hedgeRelationshipEntity.DayCountConv = dayCount;
                }
                
                if (Enum.TryParse<DerivativeEDGEDomainEntitiesEnumsPayBusDayConv>(request.PayBusDayConv, out var payBusDay))
                {
                    hedgeRelationshipEntity.PayBusDayConv = payBusDay;
                }
                
                hedgeRelationshipEntity.AdjustedDates = request.AdjustedDates;
                hedgeRelationshipEntity.MarkAsAcquisition = request.MarkAsAcquisition;

                // Call the Redesignate API endpoint
                var redesignatedHedgeRelationship = await hedgeAccountingApiClient.RedesignatePOSTAsync(
                    hedgeRelationshipEntity,
                    cancellationToken);

                logger.LogInformation("Successfully redesignated hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully re-designated", redesignatedHedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error occurred while redesignating hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}",
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                
                // Return the actual API error message to the user
                var errorMessage = !string.IsNullOrEmpty(apiEx.Response) 
                    ? $"Failed to redesignate hedge relationship: {apiEx.Response}" 
                    : $"Failed to redesignate hedge relationship. Status code: {apiEx.StatusCode}";
                
                return new Response(true, errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while redesignating hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, $"Failed to redesignate hedge relationship: {ex.Message}");
            }
        }
    }
}
