namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class DownloadTestResultExcelService
{
    public sealed record Query(
        long BatchId,
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship
    ) : IRequest<Response>;
    
    public sealed record Response(Stream ExcelStream, string FileName);
    
    public sealed class Handler(
        IHedgeAccountingApiClient hedgeAccountingApiClient, 
        ILogger<DownloadTestResultExcelService.Handler> logger, 
        IMapper mapper) : IRequestHandler<Query, Response>
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
                logger.LogInformation("Downloading test result Excel for batch ID: {BatchId}", request.BatchId);

                // Map to API entity for the export request
                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
                
                // Set the batch ID for export (legacy: model.HedgeRegressionForExport = obj.getSelectedRecords()[0].ID)
                apiEntity.HedgeRegressionForExport = request.BatchId;

                // Set required date fields to today's date (matching legacy behavior)
                var now = DateTimeOffset.Now;
                apiEntity.ValueDate = now;
                apiEntity.TimeValuesStartDate = now;
                apiEntity.TimeValuesEndDate = now;
                apiEntity.TimeValuesFrontRollDate = now;
                apiEntity.TimeValuesBackRollDate = now;

                // Call the Export API with Xlsx file type (legacy: 'HedgeRegressionBatch/Export/Xlsx')
                var fileResponse = await hedgeAccountingApiClient.ExportAsync(
                    DerivativeEDGEHAEntityEnumFileType.Xlsx, 
                    apiEntity, 
                    cancellationToken);
                
                if (fileResponse == null)
                {
                    throw new InvalidOperationException("File response was null");
                }

                var fileName = GenerateDefaultFileName(request.BatchId);

                var stream = fileResponse.Stream;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                logger.LogInformation("Successfully downloaded test result Excel: {FileName}", fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to download test result Excel for batch ID: {BatchId}", request.BatchId);
                throw;
            }
        }

        private static string GenerateDefaultFileName(long batchId)
        {
            // Format: HedgeRegressionBatch_{batchId}_{timestamp}.xlsx
            var timestamp = DateTime.Now.ToString("MMddyyyyHHmm");
            return $"HedgeRegressionBatch_{batchId}_{timestamp}.xlsx";
        }
    }
}
