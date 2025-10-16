using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

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

    public sealed class Handler : IRequestHandler<Query, Response>
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

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending request to get GL accounts for hedging. ClientID: {ClientID}, BankEntityID: {BankEntityID}.", request.ClientID, request.BankEntityID);

                var updatedApiVm = await _hedgeAccountingApiClient.GetForHedgingAllAsync(request.ClientID, request.BankEntityID, cancellationToken);

                // Map API VM to UI view model
                var updatedVm = _mapper.Map<List<DerivativeEDGEHAEntityGLAccount>>(updatedApiVm);

                _logger.LogInformation("Successfully retrieved GL accounts for hedging. ClientID: {ClientID}, BankEntityID: {BankEntityID}.", request.ClientID, request.BankEntityID);
                return new Response(false, "Successfully retrieved GL accounts for hedging", updatedVm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting GL accounts for hedging. ClientID: {ClientID}, BankEntityID: {BankEntityID}.", request.ClientID, request.BankEntityID);
                return new Response(true, "Failed to retrieve GL accounts for hedging");
            }
        }
    }
}