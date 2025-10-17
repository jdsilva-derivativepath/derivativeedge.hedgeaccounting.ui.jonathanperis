using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;
using ApiException = DerivativeEDGE.Identity.API.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class GenerateInceptionPackage
{
    public sealed record Command(
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship, 
        bool Preview = false) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public FileResponse Data { get; set; }
        
        public Response(bool hasError, string message, FileResponse data = null)
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
                _logger.LogInformation("Generating inception package for hedge relationship ID: {HedgeRelationshipId}, Preview: {Preview}", 
                    request.HedgeRelationship.ID, request.Preview);

                // Map to entity for API call
                var hedgeRelationshipEntity = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

                // Call the GenerateInceptionPackage API endpoint
                var fileResponse = await _hedgeAccountingApiClient.GenerateInceptionPackageAsync(
                    request.Preview,
                    hedgeRelationshipEntity,
                    cancellationToken);

                _logger.LogInformation("Successfully generated inception package for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);
                    
                return new Response(false, "Inception package generated successfully", fileResponse);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred while generating inception package for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}",
                    request.HedgeRelationship.ID, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to generate inception package: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating inception package for hedge relationship ID: {HedgeRelationshipId}", 
                    request.HedgeRelationship.ID);
                return new Response(true, "Failed to generate inception package due to an unexpected error");
            }
        }
    }
}
