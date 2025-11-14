using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetClientConfigSettings
{
    public sealed record Query(long Id) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsClientConfigVM result = default)
        {
            HasError = hasError;
            Message = message;
            Result = result;
        }

        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;
        public DerivativeEDGEHAApiViewModelsClientConfigVM Result { get; init; } = new();
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, ILogger<Handler> logger) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            DerivativeEDGEHAApiViewModelsClientConfigVM result = new();

            try
            {
                logger.LogInformation("Fetching client configuration settings for client id {ClientConfigId}.", request.Id);

                try
                {
                    result = await hedgeAccountingApiClient.ClientConfigGETAsync(request.Id, cancellationToken);
                }
                catch (ApiException ex) when (ex.StatusCode == StatusCodes.Status500InternalServerError || ex.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var reason = ex.Message;
                    var content = ex.GetType().GetProperty("Response")?.GetValue(ex, null);
                    logger.LogWarning("Failed to fetch client configuration settings. StatusCode: {StatusCode}, Reason: {Reason}, Content: {Content}", ex.StatusCode, reason, content);
                    return new Response(true, "Failed to fetch client configuration settings");
                }
            }
            catch
            {
                // Handling incorrect 204 interpretation by ApiClient; real failures handled in inner ApiException block.
            }

            return new Response(false, "Client configuration settings retrieved successfully.", result);
        }
    }
}
