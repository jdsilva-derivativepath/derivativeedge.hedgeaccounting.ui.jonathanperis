using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetReDesignateData
{
    public sealed record Query(long HedgeRelationshipId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAEntityHedgeRelationshipReDesignation Data { get; set; }
        public DateTime RedesignationDate { get; set; }
        public DateTime TimeValuesStartDate { get; set; }
        public DateTime TimeValuesEndDate { get; set; }
        public decimal Payment { get; set; }
        public string DayCountConv { get; set; }
        public string PayBusDayConv { get; set; }
        public string PaymentFrequency { get; set; }
        public bool AdjustedDates { get; set; }
        public bool MarkAsAcquisition { get; set; }
        
        public Response(bool hasError, string message, DerivativeEDGEHAEntityHedgeRelationshipReDesignation data = null)
        {
            HasError = hasError;
            ErrorMessage = message;
            Data = data;
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler(
        IHedgeAccountingApiClient hedgeAccountingApiClient,
        ILogger<GetReDesignateData.Handler> logger,
        TokenProvider tokenProvider) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Fetching redesignation data for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);

                // Call API to get redesignation data
                var redesignationData = await hedgeAccountingApiClient.RedesignateGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                var response = new Response(false, "Successfully retrieved redesignation data", redesignationData);
                
                // Map the redesignation data properties to response
                if (redesignationData != null)
                {
                    // Convert DateTimeOffset to DateTime
                    response.RedesignationDate = redesignationData.RedesignationDate.DateTime;
                    response.TimeValuesStartDate = redesignationData.RedesignationDate.DateTime;
                    response.TimeValuesEndDate = redesignationData.TimeValuesEndDate.DateTime;
                    
                    // response.Payment = (decimal)(redesignationData.Payment ?? 0);
                    response.DayCountConv = redesignationData.DayCountConv.ToString();
                    response.PayBusDayConv = redesignationData.PayBusDayConv.ToString();
                    response.PaymentFrequency = redesignationData.PaymentFrequency.ToString();
                    response.AdjustedDates = redesignationData.AdjustedDates;
                    response.MarkAsAcquisition = redesignationData.MarkAsAcquisition;
                }

                logger.LogInformation("Successfully fetched redesignation data for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                    
                return response;
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error occurred while fetching redesignation data for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to fetch redesignation data: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching redesignation data for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationshipId);
                return new Response(true, "Failed to fetch redesignation data due to an unexpected error");
            }
        }
    }
}
