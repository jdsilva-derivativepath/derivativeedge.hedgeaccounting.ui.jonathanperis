namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class InceptionPackageService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship, DateTime? CurveDate = null) : IRequest<Response>;
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
                
                // Set required date fields - use CurveDate if provided, otherwise use today's date (matching legacy behavior)
                // Legacy line 489: $scope.Model.ValueDate = valueDate (today's date by default, but user could change it)
                // Legacy line 2804-2805: $scope.Model.TimeValuesStartDate/EndDate = moment().format('M/D/YYYY')
                var now = DateTimeOffset.Now;
                var valueDate = request.CurveDate.HasValue ? new DateTimeOffset(request.CurveDate.Value) : now;
                apiEntity.ValueDate = valueDate;
                apiEntity.TimeValuesStartDate = now;
                apiEntity.TimeValuesEndDate = now;
                apiEntity.TimeValuesFrontRollDate = now;
                apiEntity.TimeValuesBackRollDate = now;

                // Call generated API (preview=True to match original URL query)
                var fileResponse = await hedgeAccountingApiClient.GenerateInceptionPackageAsync(true, apiEntity, cancellationToken);
                if (fileResponse == null)
                {
                    throw new InvalidOperationException("File response was null");
                }

                var fileName = GenerateDefaultFileName();

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
    }
}
