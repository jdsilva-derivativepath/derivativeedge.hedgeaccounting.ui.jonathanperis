namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

/// <summary>
/// Handler for downloading test results (regression batch) as Excel file.
/// Legacy: hr_hedgeRelationshipAddEditCtrl.js - selectedItemActionTestChanged() -> 'Download Excel' action
/// API: POST v1/HedgeRegressionBatch/Export/{ft} (FileType.Xlsx)
/// </summary>
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

                // Call the Export API with Xlsx file type (legacy: 'HedgeRegressionBatch/Export/Xlsx')
                var fileResponse = await hedgeAccountingApiClient.ExportAsync(
                    DerivativeEDGEHAEntityEnumFileType.Xlsx, 
                    apiEntity, 
                    cancellationToken);
                
                if (fileResponse == null)
                {
                    throw new InvalidOperationException("File response was null");
                }

                // Extract filename from Content-Disposition header
                var fileName = fileResponse.Headers?.TryGetValue("Content-Disposition", out var values) == true
                    ? ExtractFileName(values.FirstOrDefault())
                    : GenerateDefaultFileName(request.BatchId);

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

        private static string ExtractFileName(string contentDisposition)
        {
            if (string.IsNullOrWhiteSpace(contentDisposition))
            {
                return GenerateDefaultFileName(0);
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
                return GenerateDefaultFileName(0);
            }

            var startPos = idx + token.Length;
            var value = contentDisposition[startPos..];
            
            // Find the end of this parameter (semicolon or end of string)
            var semicolonIdx = value.IndexOf(';');
            var result = semicolonIdx >= 0 ? value[..semicolonIdx].Trim() : value.Trim();
            
            // Remove surrounding quotes if present
            result = result.Trim('"');
            
            return string.IsNullOrWhiteSpace(result) ? GenerateDefaultFileName(0) : result;
        }
    }
}
