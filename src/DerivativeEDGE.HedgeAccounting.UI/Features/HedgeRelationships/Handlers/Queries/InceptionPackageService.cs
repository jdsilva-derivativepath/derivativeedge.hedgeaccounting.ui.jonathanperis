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
                    ? ExtractFileName(values.FirstOrDefault())
                    : GenerateDefaultFileName();

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

        private static string GenerateDefaultFileName()
        {
            // Legacy format: InceptionPackage + MMDDYYYYHHmm + .zip
            // Example: InceptionPackage101720251312.zip (October 17, 2025 13:12)
            var timestamp = DateTime.Now.ToString("MMddyyyyHHmm");
            return $"InceptionPackage{timestamp}.zip";
        }

        private static string ExtractFileName(string contentDisposition)
        {
            if (string.IsNullOrWhiteSpace(contentDisposition))
            {
                return GenerateDefaultFileName();
            }

            // Try to extract filename* first (RFC 5987 encoded filename)
            const string encodedToken = "filename*=";
            var encodedIdx = contentDisposition.IndexOf(encodedToken, StringComparison.OrdinalIgnoreCase);
            if (encodedIdx >= 0)
            {
                var start = encodedIdx + encodedToken.Length;
                var remainder = contentDisposition[start..];
                
                // Find the end of this parameter (semicolon or end of string)
                var endIdx = remainder.IndexOf(';');
                var encodedValue = endIdx >= 0 ? remainder[..endIdx].Trim() : remainder.Trim();
                
                // RFC 5987 format: charset'lang'filename (e.g., UTF-8''filename.zip)
                var parts = encodedValue.Split('\'');
                if (parts.Length >= 3)
                {
                    var fileName = string.Join("'", parts.Skip(2));
                    fileName = Uri.UnescapeDataString(fileName);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        return fileName;
                    }
                }
            }

            // Fall back to filename= parameter
            const string token = "filename=";
            var idx = contentDisposition.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return GenerateDefaultFileName();
            }

            var startPos = idx + token.Length;
            var value = contentDisposition[startPos..];
            
            // Find the end of this parameter (semicolon or end of string)
            var semicolonIdx = value.IndexOf(';');
            var result = semicolonIdx >= 0 ? value[..semicolonIdx].Trim() : value.Trim();
            
            // Remove surrounding quotes if present
            result = result.Trim('"');
            
            return string.IsNullOrWhiteSpace(result) ? GenerateDefaultFileName() : result;
        }
    }
}
