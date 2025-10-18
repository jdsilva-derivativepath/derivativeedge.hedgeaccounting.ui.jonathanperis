namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

/// <summary>
/// Handler for deleting a test batch (regression batch).
/// Legacy: hr_hedgeRelationshipAddEditCtrl.js - selectedItemActionTestChanged() -> 'Delete' action
/// API: POST v1/HedgeRelationship/DeleteBatch/{batchid}
/// Returns updated hedge relationship after deletion
/// </summary>
public sealed class DeleteTestBatchService
{
    public sealed record Command(
        long BatchId,
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship
    ) : IRequest<Response>;
    
    public sealed record Response(
        bool IsSuccess,
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM UpdatedHedgeRelationship
    );
    
    public sealed class Handler(
        IHedgeAccountingApiClient hedgeAccountingApiClient,
        ILogger<DeleteTestBatchService.Handler> logger,
        IMapper mapper) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.HedgeRelationship == null)
            {
                logger.LogError("HedgeRelationship is required but was null");
                throw new ArgumentNullException(nameof(request.HedgeRelationship), "HedgeRelationship is required");
            }

            try
            {
                logger.LogInformation("Deleting test batch ID: {BatchId} for HedgeRelationship ID: {HedgeRelationshipId}", 
                    request.BatchId, request.HedgeRelationship.ID);

                // Map to API entity for the delete request (legacy: setModelData(response.data))
                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Call the DeleteBatch API (legacy: 'HedgeRelationship/DeleteBatch')
                // This returns the updated hedge relationship with the batch removed
                var updatedHedgeRelationship = await hedgeAccountingApiClient.DeleteBatchAsync(
                    request.BatchId, 
                    apiEntity, 
                    cancellationToken);
                
                if (updatedHedgeRelationship == null)
                {
                    logger.LogError("Delete batch API returned null");
                    return new Response(false, request.HedgeRelationship);
                }

                logger.LogInformation("Successfully deleted test batch ID: {BatchId}", request.BatchId);
                return new Response(true, updatedHedgeRelationship);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete test batch ID: {BatchId}", request.BatchId);
                throw;
            }
        }
    }
}
