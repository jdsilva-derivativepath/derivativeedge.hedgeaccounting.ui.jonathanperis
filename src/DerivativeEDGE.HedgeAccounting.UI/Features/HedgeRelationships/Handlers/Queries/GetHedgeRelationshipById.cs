using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetHedgeRelationshipById
{
    public sealed record Query(long HedgeId) : IRequest<Response>;

    public sealed class Response : ResponseBase
    {
        public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public Response(bool hasError, string message, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM data = null)
        {
            HasError = hasError;
            Message = message;
            Data = data;
        }
    }

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IHedgeAccountingApiClient _hedgeAccountingApiClient;
        private readonly TokenProvider _tokenProvider;
        private readonly IMapper _mapper;
        private const string ErrorMessage = "There was a system error while creating this user. Please try again, or contact support if the problem continues.";

        public Handler(IHedgeAccountingApiClient hedgeAccountingApiClient, TokenProvider tokenProvider, ILogger<Handler> logger, IMapper mapper)
        {
            _hedgeAccountingApiClient = hedgeAccountingApiClient;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(_tokenProvider.AccessToken))
                {
                    _logger.LogWarning("Access token is null or empty for HedgeId: {HedgeId}", query.HedgeId);
                    return new Response(true, ErrorMessage);
                }

                _logger.LogInformation("Fetching hedge relationship for HedgeId: {HedgeId}", query.HedgeId);

                try
                {
                    var apiVm = await _hedgeAccountingApiClient.HedgeRelationshipGETAsync(query.HedgeId, cancellationToken);
                    if (apiVm == null)
                    {
                        _logger.LogWarning("Failed to fetch hedge relationship. StatusCode: {StatusCode}", "(no content)");
                        return new Response(true, ErrorMessage);
                    }

                    _logger.LogInformation("Successfully fetched hedge relationship for HedgeId: {HedgeId}", query.HedgeId);
                    return new Response(false, "Success", apiVm);
                }
                catch (Exception ex) when (ex.GetType().Name == "ApiException")
                {
                    // Attempt to get StatusCode property via reflection to preserve original log message format
                    var statusCode = ex.GetType().GetProperty("StatusCode")?.GetValue(ex, null);
                    _logger.LogWarning("Failed to fetch hedge relationship. StatusCode: {StatusCode}", statusCode);
                    return new Response(true, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hedge relationship for HedgeId: {HedgeId}", query.HedgeId);
                return new Response(true, ErrorMessage);
            }
        }
    }
}