using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class RunRegression
{
    public sealed record Command(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship, DerivativeEDGEHAEntityEnumHedgeResultType HedgeResultType = DerivativeEDGEHAEntityEnumHedgeResultType.User) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null, List<string> validationErrors = null)
        {
            HasError = hasError;
            ErrorMessage = message;
            Data = data;
            ValidationErrors = validationErrors ?? new List<string>();
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMapper _mapper;
        private readonly TokenProvider _tokenProvider; // future use

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, IMapper mapper, ILogger<Handler> logger, TokenProvider tokenProvider)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _mapper = mapper;
            _logger = logger;
            _tokenProvider = tokenProvider; // stored for future hook
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending request to run regression for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);

                // Minimal body needed for regression (send ID + Notional + Description)
                var body = new DerivativeEDGEHAEntityHedgeRelationship
                {
                    ID = request.HedgeRelationship.ID,
                    Notional = (double)request.HedgeRelationship.Notional,
                    Description = request.HedgeRelationship.Description,
                };

                var apiResponse = await _hedgeAccountingApiClient.RegressAsync(request.HedgeResultType, body, cancellationToken);

                _logger.LogInformation("Successfully completed regression analysis for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);
                return new Response(false, "Regression analysis completed successfully", apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running regression analysis for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);
                return new Response(true, "Failed to run regression analysis due to an unexpected error");
            }
        }
    }
}
