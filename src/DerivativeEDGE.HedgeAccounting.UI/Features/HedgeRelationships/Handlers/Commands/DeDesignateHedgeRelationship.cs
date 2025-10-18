using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class DeDesignateHedgeRelationship
{
    public sealed record Command(
        long HedgeRelationshipId,
        DateTime DedesignationDate,
        int DedesignationReason,
        decimal Payment,
        DateTime TimeValuesStartDate,
        DateTime TimeValuesEndDate,
        int CashPaymentType,
        bool HedgedExposureExist,
        decimal BasisAdjustment = 0,
        decimal BasisAdjustmentBalance = 0) : IRequest<Response>;

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
        ILogger<DeDesignateHedgeRelationship.Handler> logger,
        TokenProvider tokenProvider) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to dedesignate hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                var currentHedgeRelationship = await hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                var validationErrors = DeDesignateValidator.Validate(
                    currentHedgeRelationship, 
                    request.DedesignationDate, 
                    request.DedesignationReason);

                // Additional validation for enum casting
                if (!Enum.IsDefined(typeof(DerivativeEDGEHAEntityEnumCashPaymentType), request.CashPaymentType))
                {
                    validationErrors.Add($"Invalid CashPaymentType value: {request.CashPaymentType}");
                }

                if (validationErrors.Count != 0)
                {
                    var errorMessage = string.Join("; ", validationErrors);
                    logger.LogWarning("De-designation validation failed for hedge relationship ID: {HedgeRelationshipId}. Errors: {Errors}", 
                        request.HedgeRelationshipId, errorMessage);
                    return new Response(true, errorMessage);
                }

                var hedgeRelationshipEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(currentHedgeRelationship);
                
                // Cast DateTime to DateTimeOffset / nullable DateTimeOffset without string formatting
                hedgeRelationshipEntity.DedesignationDate = new DateTimeOffset(request.DedesignationDate.Date, TimeSpan.Zero);
                hedgeRelationshipEntity.DedesignationReason = (DerivativeEDGEHAEntityEnumDedesignationReason)request.DedesignationReason;
                hedgeRelationshipEntity.Payment = (double)request.Payment;
                hedgeRelationshipEntity.TimeValuesStartDate = new DateTimeOffset(request.TimeValuesStartDate.Date, TimeSpan.Zero);
                hedgeRelationshipEntity.TimeValuesEndDate = new DateTimeOffset(request.TimeValuesEndDate.Date, TimeSpan.Zero);
                hedgeRelationshipEntity.CashPaymentType = (DerivativeEDGEHAEntityEnumCashPaymentType)request.CashPaymentType;
                hedgeRelationshipEntity.HedgedExposureExist = request.HedgedExposureExist;
                hedgeRelationshipEntity.BasisAdjustment = (double)request.BasisAdjustment;
                hedgeRelationshipEntity.BasisAdjustmentBalance = (double)request.BasisAdjustmentBalance;

                var dedesignatedHedgeRelationship = await hedgeAccountingApiClient.DedesignatePOSTAsync(
                    hedgeRelationshipEntity,
                    cancellationToken);

                logger.LogInformation("Successfully dedesignated hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully de-designated", dedesignatedHedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error occurred while dedesignating hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}",
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                
                // Return the actual API error message to the user
                var errorMessage = !string.IsNullOrEmpty(apiEx.Response) 
                    ? $"Failed to dedesignate hedge relationship: {apiEx.Response}" 
                    : $"Failed to dedesignate hedge relationship. Status code: {apiEx.StatusCode}";
                
                return new Response(true, errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while dedesignating hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, $"Failed to dedesignate hedge relationship: {ex.Message}");
            }
        }
    }
}
