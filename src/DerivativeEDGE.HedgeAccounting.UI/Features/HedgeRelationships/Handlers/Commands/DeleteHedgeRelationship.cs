namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class DeleteHedgeRelationship
{
    public sealed record Command(long HedgeRelationshipId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public Response(bool hasError, string message)
        {
            HasError = hasError;
            Message = message;
        }
        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<DeleteHedgeRelationship.Handler> logger) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending DELETE request to delete hedge relationship {HrId}.", request.HedgeRelationshipId);

                try
                {
                    await hedgeAccountingApiClient.HedgeRelationshipDELETEAsync(request.HedgeRelationshipId, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message;
                    var content = ex.GetType().GetProperty("Response")?.GetValue(ex, null);
                    logger.LogWarning("Failed to delete hedge relationship. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}", statusCode, reason, content);
                    return new Response(true, "Failed to delete hedge relationship");
                }

                logger.LogInformation("Successfully deleted hedge relationship.");
                return new Response(false, "Successfully deleted hedge relationship");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting hedge relationship.");
                return new Response(true, "Failed to delete hedge relationship");
            }
        }
    }
}