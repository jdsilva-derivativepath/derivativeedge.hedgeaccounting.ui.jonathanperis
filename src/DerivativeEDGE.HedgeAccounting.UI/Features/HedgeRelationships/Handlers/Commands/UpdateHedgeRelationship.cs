using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class UpdateHedgeRelationship
{
    public sealed record Command(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null)
        {
            HasError = hasError;
            Message = message;
            Data = data;
        }
        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<UpdateHedgeRelationship.Handler> logger, IMapper mapper) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to update hedge relationship ID {Id}.", request.HedgeRelationship.ID);

                // Map API VM to UI view model
                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Execute update (PUT). Generated client returns Task (no payload)
                await hedgeAccountingApiClient.HedgeRelationshipPUTAsync(request.HedgeRelationship.ID, apiEntity, cancellationToken);

                // Retrieve updated entity
                var updatedApiVm = await hedgeAccountingApiClient.HedgeRelationshipGETAsync(request.HedgeRelationship.ID, cancellationToken);

                logger.LogInformation("Successfully updated hedge relationship ID {Id}.", request.HedgeRelationship.ID);
                return new Response(false, "Successfully updated hedge relationship", updatedApiVm);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, "API error while updating hedge relationship ID {Id}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationship.ID, apiEx.StatusCode, apiEx.Response);
                
                // Return the actual API error message to the user
                var errorMessage = !string.IsNullOrEmpty(apiEx.Response) 
                    ? $"Failed to update hedge relationship: {apiEx.Response}" 
                    : $"Failed to update hedge relationship. Status code: {apiEx.StatusCode}";
                
                return new Response(true, errorMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating hedge relationship ID {Id}.", request.HedgeRelationship.ID);
                return new Response(true, $"Failed to update hedge relationship: {ex.Message}");
            }
        }
    }
}