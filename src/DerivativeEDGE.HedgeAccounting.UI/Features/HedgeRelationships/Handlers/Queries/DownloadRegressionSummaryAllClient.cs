namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class DownloadRegressionSummaryAllClient
{
    public sealed record Query(DateTime? ValueDate) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly TokenProvider _tokenProvider; // retained for future hooks

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<Handler> logger)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var valueDate = request.ValueDate.HasValue ? new DateTimeOffset(request.ValueDate.Value) : (DateTimeOffset?)null;

            // We avoid constructing a full URL (to prevent hard-coding path segments). For logging, capture method & base URL.
            var clientTypeName = typeof(IHedgeAccountingApiClient).Name;
            var methodName = nameof(IHedgeAccountingApiClient.RegressionSummaryAllClientsAsync);

            try
            {
                _logger.LogInformation("Initiating request via {Client}.{Method} (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);

                FileResponse fileResponse;
                try
                {
                    fileResponse = await _hedgeAccountingApiClient.RegressionSummaryAllClientsAsync(valueDate, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message;
                    _logger.LogWarning("{Client}.{Method} failed. StatusCode: {StatusCode}, Reason: {Reason}", clientTypeName, methodName, statusCode, reason);
                    throw; // preserve original behavior
                }

                if (fileResponse == null)
                {
                    _logger.LogWarning("{Client}.{Method} returned null FileResponse (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);
                    throw new InvalidOperationException("FileResponse was null");
                }

                var fileName = fileResponse.Headers?.TryGetValue("Content-Disposition", out var values) == true
                    ? ExtractFileName(values.FirstOrDefault(), "RegressionSummary.zip")
                    : "RegressionSummary.zip";

                var stream = fileResponse.Stream;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                _logger.LogInformation("{Client}.{Method} succeeded (valueDate: {ValueDate}) -> {FileName}", clientTypeName, methodName, request.ValueDate, fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                // Obtain BaseUrl reflectively only when needed for error context
                var baseUrl = _hedgeAccountingApiClient.GetType()
                    .GetProperty("BaseUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                    .GetValue(_hedgeAccountingApiClient) as string ?? string.Empty;

                _logger.LogError(ex, "Error executing {Client}.{Method} (BaseUrl: {BaseUrl}, valueDate: {ValueDate})", clientTypeName, methodName, baseUrl, request.ValueDate);
                throw;
            }
        }

        private static string ExtractFileName(string contentDisposition, string fallback)
        {
            if (string.IsNullOrWhiteSpace(contentDisposition))
            {
                return fallback;
            }
            const string token = "filename=";
            var idx = contentDisposition.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return fallback;
            }
            var remainder = contentDisposition[(idx + token.Length)..].Trim().Trim('"');
            return string.IsNullOrWhiteSpace(remainder) ? fallback : remainder;
        }
    }
}
