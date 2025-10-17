using ApiException = DerivativeEdge.HedgeAccounting.Api.Client.ApiException;

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

                // Map the full HedgeRelationshipVM to HedgeRelationship entity (following UpdateHedgeRelationship pattern)
                // The API needs the complete hedge relationship data including hedged items, hedging items, effectiveness methods, etc.
                var body = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                var apiResponse = await _hedgeAccountingApiClient.RegressAsync(request.HedgeResultType, body, cancellationToken);

                _logger.LogInformation("Successfully completed regression analysis for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);
                return new Response(false, "Regression analysis completed successfully", apiResponse);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error while running regression analysis for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}", 
                    request.HedgeRelationship.ID, apiEx.StatusCode, apiEx.Response);
                
                // Return the actual API error message to the user
                var errorMessage = !string.IsNullOrEmpty(apiEx.Response) 
                    ? $"Failed to run regression analysis: {apiEx.Response}" 
                    : $"Failed to run regression analysis. Status code: {apiEx.StatusCode}";
                
                return new Response(true, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running regression analysis for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationship.ID);
                return new Response(true, $"Failed to run regression analysis: {ex.Message}");
            }
        }
    }
}
