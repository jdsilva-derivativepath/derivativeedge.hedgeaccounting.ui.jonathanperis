using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class InceptionPackageService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);
    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly TokenProvider _tokenProvider; // future auth hook retained
        private readonly IMapper _mapper;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<Handler> logger, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate input
            if (request.HedgeRelationship == null)
            {
                _logger.LogError("HedgeRelationship is required but was null");
                throw new ArgumentNullException(nameof(request.HedgeRelationship), "HedgeRelationship is required");
            }

            // Validate token (preserve original behavior / logs)
            if (string.IsNullOrWhiteSpace(_tokenProvider.AccessToken))
            {
                _logger.LogWarning("Access token is missing or empty");
                throw new UnauthorizedAccessException("Authentication token is required");
            }

            try
            {
                _logger.LogInformation("Generating inception package for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);

                // Map to API entity (only mapped fields will be used)
                var apiEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Call generated API (preview=True to match original URL query)
                var fileResponse = await _hedgeAccountingApiClient.GenerateInceptionPackageAsync(true, apiEntity, cancellationToken);
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

                _logger.LogInformation("Successfully generated inception package: {FileName}", fileName);

                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate inception package for HedgeRelationship ID: {HedgeRelationshipId}", 
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
