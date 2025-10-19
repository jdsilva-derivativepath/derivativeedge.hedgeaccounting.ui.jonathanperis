namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class ExportAmortizationScheduleService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship, long SelectedAmortizationId) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);
    
    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<ExportAmortizationScheduleService.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
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
                logger.LogInformation("Exporting amortization schedule for HedgeRelationship ID: {HedgeRelationshipId}, Amortization ID: {AmortizationId}", 
                    request.HedgeRelationship.ID, request.SelectedAmortizationId);

                // Set the selected amortization ID on the hedge relationship
                var hedgeRelationship = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
                hedgeRelationship.SelectedHedgeRelationshipOptionTimeValueAmortID = request.SelectedAmortizationId;

                // Call API endpoint (legacy: ExportHedgeAmortizatonSchedule)
                var fileResponse = await hedgeAccountingApiClient.ExportHedgeAmortizatonScheduleAsync(hedgeRelationship, cancellationToken);
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

                logger.LogInformation("Successfully exported amortization schedule: {FileName}", fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to export amortization schedule for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);
                throw;
            }
        }

        private static string GenerateDefaultFileName()
        {
            // Legacy format: HedgeAmortizatonSchedule + MMDDYYYYHHmm + .xlsx
            // Example: HedgeAmortizatonSchedule101720251312.xlsx (October 17, 2025 13:12)
            var timestamp = DateTime.Now.ToString("MMddyyyyHHmm");
            return $"HedgeAmortizatonSchedule{timestamp}.xlsx";
        }
    }
}
