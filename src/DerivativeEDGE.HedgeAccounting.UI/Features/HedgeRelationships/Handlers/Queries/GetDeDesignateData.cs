using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetDeDesignateData
{
    public sealed record Query(long HedgeRelationshipId, DerivativeEDGEHAEntityEnumDedesignationReason Reason) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAEntityHedgeRelationshipDeDesignation Data { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime DedesignationDate { get; set; }
        public DateTime TimeValuesStartDate { get; set; }
        public DateTime TimeValuesEndDate { get; set; }
        public decimal Payment { get; set; }
        public bool ShowBasisAdjustmentBalance { get; set; }
        public decimal BasisAdjustment { get; set; }
        public decimal BasisAdjustmentBalance { get; set; }
        
        public Response(bool hasError, string message, DerivativeEDGEHAEntityHedgeRelationshipDeDesignation data = null)
        {
            HasError = hasError;
            base.ErrorMessage = message;
            Data = data;
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
                _logger.LogInformation("Fetching dedesignation data for hedge relationship ID: {HedgeRelationshipId} with reason: {Reason}", 
                    request.HedgeRelationshipId, request.Reason);

                // Call API to get dedesignation data
                var dedesignationData = await _hedgeAccountingApiClient.DedesignateGETAsync(
                    request.HedgeRelationshipId,
                    request.Reason,
                    cancellationToken);

                var response = new Response(false, "Successfully retrieved dedesignation data", dedesignationData);
                
                // Map the dedesignation data properties to response
                if (dedesignationData != null)
                {
                    // Parse dates from the API response
                    if (DateTime.TryParse(dedesignationData.DedesignationDate, out var dedesignDate))
                    {
                        response.DedesignationDate = dedesignDate;
                        response.TimeValuesStartDate = dedesignDate;
                    }
                    
                    if (DateTime.TryParse(dedesignationData.TimeValuesEndDate, out var timeValueEndDate))
                    {
                        response.TimeValuesEndDate = timeValueEndDate;
                    }
                    
                    response.Payment = (decimal)(dedesignationData.Payment ?? 0);
                    response.ShowBasisAdjustmentBalance = dedesignationData.ShowBasisAdjustmentBalance;
                    response.BasisAdjustment = (decimal)(dedesignationData.BasisAdjustment ?? 0);
                    response.BasisAdjustmentBalance = (decimal)(dedesignationData.BasisAdjustmentBalance ?? 0);
                    response.ErrorMessage = dedesignationData.UserMessage;
                }

                _logger.LogInformation("Successfully fetched dedesignation data for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                    
                return response;
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while fetching dedesignation data for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to fetch dedesignation data: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching dedesignation data for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                return new Response(true, "Failed to fetch dedesignation data due to an unexpected error");
            }
        }
    }
}
