namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class CheckAnalyticsStatus
{
    public sealed record Query : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public bool IsAnalyticsAvailable { get; set; }

        public Response(bool hasError, string message, bool isAnalyticsAvailable = false)
        {
            HasError = hasError;
            ErrorMessage = message;
            IsAnalyticsAvailable = isAnalyticsAvailable;
        }

        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<CheckAnalyticsStatus.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Checking analytics service availability");

                // Directly call generated API client
                var isAvailable = await hedgeAccountingApiClient.IsAnalyticsAvailableAsync(cancellationToken);

                logger.LogInformation("Analytics service availability check completed. Available: {IsAvailable}", isAvailable);
                return new Response(false, "Analytics status checked successfully", isAvailable);
            }
            catch (Exception ex)
            {
                // If we can detect an HTTP status (e.g., ApiException) we mimic the previous warning log
                if (ex.GetType().Name == "ApiException")
                {
                    // Attempt to extract status code via reflection to keep original warning message format
                    var statusCodeProp = ex.GetType().GetProperty("StatusCode");
                    var statusCodeVal = statusCodeProp?.GetValue(ex, null);
                    logger.LogWarning("Failed to check analytics status. StatusCode: {StatusCode}, Reason: {ReasonPhrase}", statusCodeVal, ex.Message);
                    return new Response(true, "Failed to check analytics service status", false);
                }

                logger.LogError(ex, "An error occurred while checking analytics service availability");
                return new Response(true, "Failed to check analytics service status due to an unexpected error", false);
            }
        }
    }
}
