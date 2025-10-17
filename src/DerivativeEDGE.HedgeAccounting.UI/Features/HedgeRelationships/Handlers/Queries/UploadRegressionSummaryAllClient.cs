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

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly TokenProvider _tokenProvider; // retained

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<Handler> logger)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var clientTypeName = typeof(IHedgeAccountingApiClient).Name;
            var methodName = nameof(IHedgeAccountingApiClient.ProcessSummaryAllClientsAsync);

            try
            {
                _logger.LogInformation("Initiating request via {Client}.{Method} (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);

                try
                {
                    var valueDate = request.ValueDate.HasValue ? new DateTimeOffset(request.ValueDate.Value) : (DateTimeOffset?)null;
                    await _hedgeAccountingApiClient.ProcessSummaryAllClientsAsync(valueDate, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message;
                    _logger.LogWarning("{Client}.{Method} failed. StatusCode: {StatusCode}, Reason: {Reason}", clientTypeName, methodName, statusCode, reason);
                    throw; // maintain original semantics
                }

                _logger.LogInformation("{Client}.{Method} succeeded (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);
                return new Response(false, "Successfully uploaded regression summary");
            }
            catch (Exception ex)
            {
                var baseUrl = _hedgeAccountingApiClient.GetType()
                    .GetProperty("BaseUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                    .GetValue(_hedgeAccountingApiClient) as string ?? string.Empty;
                _logger.LogError(ex, "Error executing {Client}.{Method} (BaseUrl: {BaseUrl}, valueDate: {ValueDate})", clientTypeName, methodName, baseUrl, request.ValueDate);
                return new Response(true, "Failed to upload regression summary");
            }
        }
    }
}
