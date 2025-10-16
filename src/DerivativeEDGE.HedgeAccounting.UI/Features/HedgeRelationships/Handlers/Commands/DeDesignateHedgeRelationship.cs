using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

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

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMapper _mapper;
        private readonly TokenProvider _tokenProvider;

        public Handler(
            IHedgeAccountingApiClient hedgeAccountingApiClient,
            IMapper mapper,
            ILogger<Handler> logger,
            TokenProvider tokenProvider)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _mapper = mapper;
            _logger = logger;
            _tokenProvider = tokenProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending request to dedesignate hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                // Get the current hedge relationship
                var currentHedgeRelationship = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                // Map to entity and update dedesignation properties
                var hedgeRelationshipEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(currentHedgeRelationship);
                
                // Set dedesignation specific properties
                hedgeRelationshipEntity.DedesignationDate = request.DedesignationDate.ToString("MM/dd/yyyy");
                hedgeRelationshipEntity.DedesignationReason = (DerivativeEDGEHAEntityEnumDedesignationReason)request.DedesignationReason;
                hedgeRelationshipEntity.Payment = (double)request.Payment;
                hedgeRelationshipEntity.TimeValuesStartDate = request.TimeValuesStartDate.ToString("MM/dd/yyyy");
                hedgeRelationshipEntity.TimeValuesEndDate = request.TimeValuesEndDate.ToString("MM/dd/yyyy");
                hedgeRelationshipEntity.CashPaymentType = request.CashPaymentType;
                hedgeRelationshipEntity.HedgedExposureExist = request.HedgedExposureExist;
                hedgeRelationshipEntity.BasisAdjustment = (double)request.BasisAdjustment;
                hedgeRelationshipEntity.BasisAdjustmentBalance = (double)request.BasisAdjustmentBalance;

                // Call the Dedesignate API endpoint
                var dedesignatedHedgeRelationship = await _hedgeAccountingApiClient.DedesignatePOSTAsync(
                    hedgeRelationshipEntity,
                    cancellationToken);

                _logger.LogInformation("Successfully dedesignated hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully de-designated", dedesignatedHedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while dedesignating hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}",
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to dedesignate hedge relationship: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while dedesignating hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, "Failed to dedesignate hedge relationship due to an unexpected error");
            }
        }
    }
}
