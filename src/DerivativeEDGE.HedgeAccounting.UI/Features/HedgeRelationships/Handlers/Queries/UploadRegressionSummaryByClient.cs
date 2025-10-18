namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class UploadRegressionSummaryByClient
{
    public sealed record Query(long? id, DateTime? ValueDate) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public Response(bool hasError, string message)
        {
            HasError = hasError;
            Message = message;
        }
        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<UploadRegressionSummaryByClient.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var clientTypeName = typeof(IHedgeAccountingApiClient).Name;
            var methodName = nameof(IHedgeAccountingApiClient.ProcessSummaryAsync);

            try
            {
                logger.LogInformation("Initiating request via {Client}.{Method} (id: {Id}, valueDate: {ValueDate})", clientTypeName, methodName, request.id, request.ValueDate);

                try
                {
                    var valueDate = request.ValueDate.HasValue ? new DateTimeOffset(request.ValueDate.Value) : (DateTimeOffset?)null;
                    await hedgeAccountingApiClient.ProcessSummaryAsync(request.id ?? 0, valueDate, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message;
                    logger.LogWarning("{Client}.{Method} failed. StatusCode: {StatusCode}, Reason: {Reason}", clientTypeName, methodName, statusCode, reason);
                    throw; // preserve semantics
                }

                logger.LogInformation("{Client}.{Method} succeeded (id: {Id}, valueDate: {ValueDate})", clientTypeName, methodName, request.id, request.ValueDate);
                return new Response(false, "Successfully uploaded regression summary");
            }
            catch (Exception ex)
            {
                var baseUrl = hedgeAccountingApiClient.GetType()
                    .GetProperty("BaseUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                    .GetValue(hedgeAccountingApiClient) as string ?? string.Empty;
                logger.LogError(ex, "Error executing {Client}.{Method} (BaseUrl: {BaseUrl}, id: {Id}, valueDate: {ValueDate})", clientTypeName, methodName, baseUrl, request.id, request.ValueDate);
                return new Response(true, "Failed to upload regression summary");
            }
        }
    }
}
