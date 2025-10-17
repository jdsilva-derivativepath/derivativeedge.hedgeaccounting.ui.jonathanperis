using ApiException = DerivativeEDGE.Identity.API.Client.ApiException;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;

public sealed class DesignateHedgeRelationship
{
    public sealed record Command(long HedgeRelationshipId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public FileResponse InceptionPackage { get; set; }
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
        
        public Response(bool hasError, string message, FileResponse inceptionPackage = null, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM hedgeRelationship = null)
        {
            HasError = hasError;
            ErrorMessage = message;
            InceptionPackage = inceptionPackage;
            HedgeRelationship = hedgeRelationship;
        }
        
        public Response(Exception exception) : base(exception) { }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly IMediator _mediator;
        private readonly TokenProvider _tokenProvider;

        public Handler(
            IHedgeAccountingApiClient hedgeAccountingApiClient,
            IMediator mediator,
            ILogger<Handler> logger,
            TokenProvider tokenProvider)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _mediator = mediator;
            _logger = logger;
            _tokenProvider = tokenProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting designation process for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);

                // Step 1: Check if document template exists
                var documentTemplateResponse = await _mediator.Send(
                    new FindDocumentTemplate.Query(request.HedgeRelationshipId), 
                    cancellationToken);

                if (documentTemplateResponse.HasError)
                {
                    return new Response(true, documentTemplateResponse.ErrorMessage);
                }

                // Step 2: Get the current hedge relationship
                var hedgeRelationship = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                // Step 3: Run regression for inception
                _logger.LogInformation("Running regression for inception for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                var regressionResponse = await _mediator.Send(
                    new RunRegression.Command(hedgeRelationship, DerivativeEDGEHAEntityEnumHedgeResultType.Inception),
                    cancellationToken);

                if (regressionResponse.HasError)
                {
                    return new Response(true, $"Regression failed: {regressionResponse.ErrorMessage}");
                }

                // Update hedge relationship with regression results
                hedgeRelationship = regressionResponse.Data;

                // Step 4: Check analytics availability
                var analyticsAvailable = await _hedgeAccountingApiClient.IsAnalyticsAvailableAsync(cancellationToken);
                
                if (!analyticsAvailable)
                {
                    _logger.LogWarning("Analytics are not available for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                }

                // Step 5: Generate inception package
                _logger.LogInformation("Generating inception package for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                var inceptionPackageResponse = await _mediator.Send(
                    new GenerateInceptionPackage.Command(hedgeRelationship, Preview: false),
                    cancellationToken);

                if (inceptionPackageResponse.HasError)
                {
                    return new Response(true, $"Failed to generate inception package: {inceptionPackageResponse.ErrorMessage}");
                }

                // Step 6: Reload the hedge relationship to get the updated state
                hedgeRelationship = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(
                    request.HedgeRelationshipId,
                    cancellationToken);

                _logger.LogInformation("Successfully completed designation process for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(false, "Hedge Relationship successfully designated", inceptionPackageResponse.Data, hedgeRelationship);
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error occurred during designation for hedge relationship ID: {HedgeRelationshipId}. Status: {StatusCode}, Response: {Response}",
                    request.HedgeRelationshipId, apiEx.StatusCode, apiEx.Response);
                return new Response(true, $"Failed to designate hedge relationship: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during designation for hedge relationship ID: {HedgeRelationshipId}", request.HedgeRelationshipId);
                return new Response(true, "Failed to designate hedge relationship due to an unexpected error");
            }
        }
    }
}
