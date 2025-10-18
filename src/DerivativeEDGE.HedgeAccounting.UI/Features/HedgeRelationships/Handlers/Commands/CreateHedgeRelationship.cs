namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class CreateHedgeRelationship
{
    public sealed record Command(DerivativeEDGEHAEntityHedgeRelationship HedgeRelationship) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAEntityHedgeRelationship Data { get; set; }
        public Response(bool hasError, string message, DerivativeEDGEHAEntityHedgeRelationship data = null)
        {
            HasError = hasError;
            Message = message;
            Data = data;
        }
        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;

    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<CreateHedgeRelationship.Handler> logger, IMapper mapper) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to create hedge relationship.");

                // Map DTO to API entity
                var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                try
                {
                    await hedgeAccountingApiClient.HedgeRelationshipPOSTAsync(apiEntity, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    // Mimic original warning log signature
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message; // no direct ReasonPhrase in generated exception
                    var content = ex.GetType().GetProperty("Response")?.GetValue(ex, null);
                    logger.LogWarning("Failed to create hedge relationship. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}", statusCode, reason, content);
                    return new Response(true, "Failed to create hedge relationship");
                }

                DerivativeEDGEHAEntityHedgeRelationship createdVm = null;
                // If the client assigns an ID (some APIs echo back in body but spec shows void). If caller provided ID we can fetch.
                if (request.HedgeRelationship.ID > 0)
                {
                    try
                    {
                        var apiVm = await hedgeAccountingApiClient.HedgeRelationshipGETAsync(request.HedgeRelationship.ID, cancellationToken);
                        createdVm = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(apiVm);
                    }
                    catch
                    {
                        // Swallow fetch errors to keep original success logging pattern
                    }
                }

                logger.LogInformation("Successfully created hedge relationship.");
                return new Response(false, "Successfully created hedge relationship", createdVm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating hedge relationship.");
                return new Response(true, "Failed to create hedge relationship");
            }
        }
    }
}