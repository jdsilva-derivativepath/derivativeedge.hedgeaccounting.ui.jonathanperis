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
                    : "SpecsAndChecks.xlsx";

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

        private static string ExtractFileName(string contentDisposition)
        {
            if (string.IsNullOrWhiteSpace(contentDisposition))
            {
                return "SpecsAndChecks.xlsx";
            }
            // naive parse for filename= pattern
            const string token = "filename=";
            var idx = contentDisposition.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return "SpecsAndChecks.xlsx";
            }
            var remainder = contentDisposition[(idx + token.Length)..].Trim().Trim('"');
            return string.IsNullOrWhiteSpace(remainder) ? "SpecsAndChecks.xlsx" : remainder;
        }
    }
}
