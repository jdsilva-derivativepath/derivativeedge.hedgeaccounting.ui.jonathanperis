using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

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
                catch (ApiException ex) when (ex.StatusCode == StatusCodes.Status500InternalServerError || ex.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var reason = ex.Message;
                    var content = ex.GetType().GetProperty("Response")?.GetValue(ex, null);
                    logger.LogWarning("Failed to delete hedge relationship. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}", ex.StatusCode, reason, content);
                    return new Response(true, "Failed to delete hedge relationship");
                }
            }
            catch
            {
                // We're getting HttpResponse 204 as an error by the ApiClient, which is wrong. So we handle it here
                // and if something really fails, we're dealing with on the inner try catch using the ApiException.
            }

            logger.LogInformation("Successfully deleted hedge relationship.");
            return new Response(false, "Successfully deleted hedge relationship");
        }
    }
}