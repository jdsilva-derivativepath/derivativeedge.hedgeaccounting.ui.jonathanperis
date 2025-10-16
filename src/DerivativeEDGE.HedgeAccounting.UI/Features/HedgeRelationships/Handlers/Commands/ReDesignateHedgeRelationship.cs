using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

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
                _logger.LogInformation("Sending request to redesignate hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                // Get the current hedge relationship
                var currentHedgeRelationship = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                // Map to entity and update redesignation properties
                var hedgeRelationshipEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(currentHedgeRelationship);
                
                // Set redesignation specific properties
                hedgeRelationshipEntity.RedesignationDate = request.RedesignationDate.ToString("MM/dd/yyyy");
                hedgeRelationshipEntity.Payment = (double)request.Payment;
                hedgeRelationshipEntity.TimeValuesStartDate = request.TimeValuesStartDate.ToString("MM/dd/yyyy");
                hedgeRelationshipEntity.TimeValuesEndDate = request.TimeValuesEndDate.ToString("MM/dd/yyyy");
                
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
                var redesignatedHedgeRelationship = await _hedgeAccountingApiClient.RedesignatePOSTAsync(
                    hedgeRelationshipEntity,
                    cancellationToken);

                _logger.LogInformation("Successfully redesignated hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully re-designated", redesignatedHedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while redesignating hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}",
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to redesignate hedge relationship: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while redesignating hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, "Failed to redesignate hedge relationship due to an unexpected error");
            }
        }
    }
}
