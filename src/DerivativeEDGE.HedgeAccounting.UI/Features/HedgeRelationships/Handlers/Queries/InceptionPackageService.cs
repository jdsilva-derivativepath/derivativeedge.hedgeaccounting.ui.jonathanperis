namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class InceptionPackageService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);
    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<InceptionPackageService.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate input
            if (request.HedgeRelationship == null)
            {
                logger.LogError("HedgeRelationship is required but was null");
                throw new ArgumentNullException(nameof(request.HedgeRelationship), "HedgeRelationship is required");
            }

            // Validate token (preserve original behavior / logs)
            if (string.IsNullOrWhiteSpace(tokenProvider.AccessToken))
            {
                logger.LogWarning("Access token is missing or empty");
                throw new UnauthorizedAccessException("Authentication token is required");
            }

            try
            {
                logger.LogInformation("Generating inception package for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);

                // Map to API entity (only mapped fields will be used)
                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Call generated API (preview=True to match original URL query)
                var fileResponse = await hedgeAccountingApiClient.GenerateInceptionPackageAsync(true, apiEntity, cancellationToken);
                if (fileResponse == null)
                {
                    throw new InvalidOperationException("File response was null");
                }

                var fileName = fileResponse.Headers?.TryGetValue("Content-Disposition", out var values) == true
                    ? ExtractFileName(values.FirstOrDefault(), "InceptionPackage.zip")
                    : "InceptionPackage.zip";

                var stream = fileResponse.Stream;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                logger.LogInformation("Successfully generated inception package: {FileName}", fileName);

                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate inception package for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);
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
