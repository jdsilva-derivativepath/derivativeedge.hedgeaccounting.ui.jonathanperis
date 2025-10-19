namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class ExportOptionAmortizationScheduleService
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship, long SelectedAmortizationId, string OptionTimeValueAmortType) : IRequest<Response>;
    public sealed record Response(Stream ExcelStream, string FileName);
    
    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<ExportOptionAmortizationScheduleService.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
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
                logger.LogInformation("Exporting option amortization schedule for HedgeRelationship ID: {HedgeRelationshipId}, Amortization ID: {AmortizationId}, Type: {AmortType}", 
                    request.HedgeRelationship.ID, request.SelectedAmortizationId, request.OptionTimeValueAmortType);

                // Set the selected amortization ID on the hedge relationship
                var hedgeRelationship = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
                hedgeRelationship.SelectedHedgeRelationshipOptionTimeValueAmortID = request.SelectedAmortizationId;

                // Call API endpoint (legacy: ExportHedgeOptionAmortizationSchedule)
                var fileResponse = await hedgeAccountingApiClient.ExportHedgeOptionAmortizationScheduleAsync(hedgeRelationship, cancellationToken);
                if (fileResponse == null)
                {
                    throw new InvalidOperationException("File response was null");
                }

                var fileName = GenerateDefaultFileName(request.OptionTimeValueAmortType);

                var stream = fileResponse.Stream;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                logger.LogInformation("Successfully exported option amortization schedule: {FileName}", fileName);
                return new Response(stream, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to export option amortization schedule for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);
                throw;
            }
        }

        private static string GenerateDefaultFileName(string optionTimeValueAmortType)
        {
            // Legacy format: 
            // - "HedgeOptionTVAmortizatonSchedule" for OptionTimeValue type
            // - "HedgeOptionIVAmortizatonSchedule" for IntrinsicValue type
            // + MMDDYYYYHHmm + .xlsx
            // Example: HedgeOptionTVAmortizatonSchedule101720251312.xlsx (October 17, 2025 13:12)
            var timestamp = DateTime.Now.ToString("MMddyyyyHHmm");
            var prefix = optionTimeValueAmortType == "OptionTimeValue" 
                ? "HedgeOptionTVAmortizatonSchedule" 
                : "HedgeOptionIVAmortizatonSchedule";
            return $"{prefix}{timestamp}.xlsx";
        }
    }
}
