namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class DownloadSpecsAndChecksService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);
    
    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMapper _mapper;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<Handler> logger, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.HedgeRelationship == null)
            {
                _logger.LogError("HedgeRelationship is required but was null");
                throw new ArgumentNullException(nameof(request.HedgeRelationship), "HedgeRelationship is required");
            }

            try
            {
                _logger.LogInformation("Downloading specs and checks for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);

                var apiEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                var fileResponse = await _hedgeAccountingApiClient.DownloadSpecsAndChecksAsync(apiEntity, cancellationToken);
                if (fileResponse == null)
                {
                    throw new InvalidOperationException("File response was null");
                }

                // NSwag FileResponse pattern: contains Stream and a Headers collection
                var fileName = fileResponse.Headers?.TryGetValue("Content-Disposition", out var values) == true
                    ? ExtractFileName(values.FirstOrDefault())
                    : GenerateDefaultFileName();

                var stream = fileResponse.Stream;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                _logger.LogInformation("Successfully downloaded specs and checks: {FileName}", fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download specs and checks for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);
                throw;
            }
        }

        private static string GenerateDefaultFileName()
        {
            // Legacy format: HRSpecsAndChecks + MMDDYYYYHHmm + .xlsx
            // Example: HRSpecsAndChecks101720251312.xlsx (October 17, 2025 13:12)
            var timestamp = DateTime.Now.ToString("MMddyyyyHHmm");
            return $"HRSpecsAndChecks{timestamp}.xlsx";
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
                
                // RFC 5987 format: charset'lang'filename (e.g., UTF-8''filename.xlsx)
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
