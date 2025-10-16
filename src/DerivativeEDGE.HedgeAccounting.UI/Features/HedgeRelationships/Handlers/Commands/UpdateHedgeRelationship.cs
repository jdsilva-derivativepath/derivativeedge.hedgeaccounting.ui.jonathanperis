using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

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

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly TokenProvider _tokenProvider; // future use if direct auth needed
        private readonly IMapper _mapper;

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<Handler> logger, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending request to update hedge relationship ID {Id}.", request.HedgeRelationship.ID);

                // Map API VM to UI view model
                var apiEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Execute update (PUT). Generated client returns Task (no payload)
                await _hedgeAccountingApiClient.HedgeRelationshipPUTAsync(request.HedgeRelationship.ID, apiEntity, cancellationToken);

                // Retrieve updated entity
                var updatedApiVm = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(request.HedgeRelationship.ID, cancellationToken);

                _logger.LogInformation("Successfully updated hedge relationship ID {Id}.", request.HedgeRelationship.ID);
                return new Response(false, "Successfully updated hedge relationship", updatedApiVm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating hedge relationship ID {Id}.", request.HedgeRelationship.ID);
                return new Response(true, "Failed to update hedge relationship");
            }
        }
    }
}