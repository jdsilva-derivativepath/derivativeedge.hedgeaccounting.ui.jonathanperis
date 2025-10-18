namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetOptionAmortizationDefaults
{
    public sealed record Query(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public Response(bool hasError, string message)
        {
            HasError = hasError;
            Message = message;
        }

        public Response(Exception exception) : base(exception) { }
        
        public DerivativeEDGEHAEntityValueObjectsOptionAmortizationDefaultValues? DefaultValues { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<GetOptionAmortizationDefaults.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to get option amortization defaults.");

                var hedgeRelationship = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
                
                var defaultValues = await hedgeAccountingApiClient.GetOptionAmortizationDefaultsAsync(hedgeRelationship, cancellationToken);

                logger.LogInformation("Successfully retrieved option amortization defaults.");
                return new Response(false, "Successfully retrieved option amortization defaults")
                {
                    DefaultValues = defaultValues
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting option amortization defaults.");
                return new Response(true, "Failed to get option amortization defaults");
            }
        }
    }
}
