namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class DownloadRegressionSummaryAllClient
{
    public sealed record Query(DateTime? ValueDate) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<DownloadRegressionSummaryAllClient.Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var valueDate = request.ValueDate.HasValue ? new DateTimeOffset(request.ValueDate.Value) : (DateTimeOffset?)null;

            // We avoid constructing a full URL (to prevent hard-coding path segments). For logging, capture method & base URL.
            var clientTypeName = typeof(IHedgeAccountingApiClient).Name;
            var methodName = nameof(IHedgeAccountingApiClient.RegressionSummaryAllClientsAsync);

            try
            {
                logger.LogInformation("Initiating request via {Client}.{Method} (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);

                FileResponse fileResponse;
                try
                {
                    fileResponse = await hedgeAccountingApiClient.RegressionSummaryAllClientsAsync(valueDate, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message;
                    logger.LogWarning("{Client}.{Method} failed. StatusCode: {StatusCode}, Reason: {Reason}", clientTypeName, methodName, statusCode, reason);
                    throw; // preserve original behavior
                }

                if (fileResponse == null)
                {
                    logger.LogWarning("{Client}.{Method} returned null FileResponse (valueDate: {ValueDate})", clientTypeName, methodName, request.ValueDate);
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

                logger.LogInformation("{Client}.{Method} succeeded (valueDate: {ValueDate}) -> {FileName}", clientTypeName, methodName, request.ValueDate, fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                // Obtain BaseUrl reflectively only when needed for error context
                var baseUrl = hedgeAccountingApiClient.GetType()
                    .GetProperty("BaseUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                    .GetValue(hedgeAccountingApiClient) as string ?? string.Empty;

                logger.LogError(ex, "Error executing {Client}.{Method} (BaseUrl: {BaseUrl}, valueDate: {ValueDate})", clientTypeName, methodName, baseUrl, request.ValueDate);
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

            // Find the first filename= occurrence and extract up to the next semicolon or end of string
            var startIdx = idx + token.Length;
            var endIdx = contentDisposition.IndexOf(';', startIdx);
            var fileNamePart = endIdx >= 0
                ? contentDisposition[startIdx..endIdx]
                : contentDisposition[startIdx..];

            var fileName = fileNamePart.Trim().Trim('"');

            // If filename is empty, fallback
            return string.IsNullOrWhiteSpace(fileName) ? fallback : fileName;
        }
    }
}
