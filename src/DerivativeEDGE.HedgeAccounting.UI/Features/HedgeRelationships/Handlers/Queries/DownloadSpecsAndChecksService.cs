namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class DownloadSpecsAndChecksService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);
    
    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<DownloadSpecsAndChecksService.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.HedgeRelationship == null)
            {
                logger.LogError("HedgeRelationship is required but was null");
                throw new ArgumentNullException(nameof(request.HedgeRelationship), "HedgeRelationship is required");
            }

            try
            {
                logger.LogInformation("Downloading specs and checks for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);

                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
                
                // Set ValueDate to today's date (matching legacy behavior)
                apiEntity.ValueDate = DateTimeOffset.Now;

                var fileResponse = await hedgeAccountingApiClient.DownloadSpecsAndChecksAsync(apiEntity, cancellationToken);
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

                logger.LogInformation("Successfully downloaded specs and checks: {FileName}", fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to download specs and checks for HedgeRelationship ID: {HedgeRelationshipId}", 
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
    }
}
