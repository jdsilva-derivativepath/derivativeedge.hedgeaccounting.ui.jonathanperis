namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetGLAccountsForHedging
{
    public sealed record Query(long ClientID, long BankEntityID) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public List<DerivativeEDGEHAEntityGLAccount> Data { get; set; }
        public Response(bool hasError, string message, List<DerivativeEDGEHAEntityGLAccount> data = null)
        {
            HasError = hasError;
            Message = message;
            Data = data;
        }
        public Response(Exception exception) : base(exception) { }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<GetGLAccountsForHedging.Handler> logger, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Sending request to get GL accounts for hedging. ClientID: {ClientID}, BankEntityID: {BankEntityID}.", request.ClientID, request.BankEntityID);

                var updatedApiVm = await hedgeAccountingApiClient.GetForHedgingAllAsync(request.ClientID, request.BankEntityID, cancellationToken);

                // Map API VM to UI view model
                var updatedVm = mapper.Map<List<DerivativeEDGEHAEntityGLAccount>>(updatedApiVm);

                logger.LogInformation("Successfully retrieved GL accounts for hedging. ClientID: {ClientID}, BankEntityID: {BankEntityID}.", request.ClientID, request.BankEntityID);
                return new Response(false, "Successfully retrieved GL accounts for hedging", updatedVm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting GL accounts for hedging. ClientID: {ClientID}, BankEntityID: {BankEntityID}.", request.ClientID, request.BankEntityID);
                return new Response(true, "Failed to retrieve GL accounts for hedging");
            }
        }
    }
}