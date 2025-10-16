using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class RedraftHedgeRelationship
{
    public sealed record Command(long HedgeRelationshipId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null)
        {
            HasError = hasError;
            ErrorMessage = message;
            Data = data;
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMapper _mapper;
        private readonly TokenProvider _tokenProvider;

        public Handler(
            IHedgeAccountingApiClient hedgeAccountingApiClient,
            IMapper mapper,
            ILogger<Handler> logger,
            TokenProvider tokenProvider)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _mapper = mapper;
            _logger = logger;
            _tokenProvider = tokenProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending request to redraft hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                // First, get the current hedge relationship
                var currentHedgeRelationship = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId, 
                    cancellationToken);

                // Map to entity for API call
                var hedgeRelationshipEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(currentHedgeRelationship);

                // Call the Redraft API endpoint
                var redraftedHedgeRelationship = await _hedgeAccountingApiClient.RedraftAsync(
                    hedgeRelationshipEntity, 
                    cancellationToken);

                _logger.LogInformation("Successfully redrafted hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully redrafted", redraftedHedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while redrafting hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to redraft hedge relationship: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while redrafting hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, "Failed to redraft hedge relationship due to an unexpected error");
            }
        }
    }
}
