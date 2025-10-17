namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class CreateHedgeRelationshipOptionTimeValueAmort
{
    public sealed record Command(DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM HedgeRelationshipOptionTimeValueAmort, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;

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

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMapper _mapper;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<Handler> logger, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending request to create hedge relationship amortization.");

                try
                {
                    var createdEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationshipOptionTimeValueAmort>(request.HedgeRelationshipOptionTimeValueAmort);
                    var hedgeRelationship = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                    createdEntity.HedgeRelationship = hedgeRelationship;

                    await _hedgeAccountingApiClient.HedgeRelationshipOptionTimeValueAmortPOSTAsync(createdEntity, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    // Mimic original warning log signature
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    var reason = ex.Message; // no direct ReasonPhrase in generated exception
                    var content = ex.GetType().GetProperty("Response")?.GetValue(ex, null);
                    _logger.LogWarning("Failed to create hedge relationship amortization. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Content: {Content}", statusCode, reason, content);
                    throw;
                }

                _logger.LogInformation("Successfully created hedge relationship amortization.");
                return new Response(false, "Successfully created hedge relationship amortization");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating hedge relationship.");
                return new Response(true, "Failed to create hedge relationship amortization");
            }
        }
    }
}