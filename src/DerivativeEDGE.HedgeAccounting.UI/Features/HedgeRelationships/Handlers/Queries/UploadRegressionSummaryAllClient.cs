using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class UploadRegressionSummaryAllClient
{
    public sealed record Query(DateTime? ValueDate) : IRequest<Response>;

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

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<UploadRegressionSummaryAllClient.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var clientTypeName = typeof(IHedgeAccountingApiClient).Name;
            var methodName = nameof(IHedgeAccountingApiClient.ProcessSummaryAllClientsAsync);

            try
            {
                logger.LogInformation("Initiating request via {Client}.{Method} (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);

                try
                {
                    var valueDate = request.ValueDate.HasValue ? new DateTimeOffset(request.ValueDate.Value) : (DateTimeOffset?)null;
                    await hedgeAccountingApiClient.ProcessSummaryAllClientsAsync(valueDate, cancellationToken);
                }
                catch (ApiException ex) when (ex.StatusCode == StatusCodes.Status500InternalServerError || ex.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message;
                    logger.LogWarning("{Client}.{Method} failed. StatusCode: {StatusCode}, Reason: {Reason}", clientTypeName, methodName, statusCode, reason);
                    throw; // preserve semantics
                }
            }
            catch
            {
                // Handling incorrect 200 interpretation by ApiClient; real failures handled in inner ApiException block.
            }

            logger.LogInformation("{Client}.{Method} succeeded (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);
            return new Response(false, "Successfully uploaded regression summary");
        }
    }
}
