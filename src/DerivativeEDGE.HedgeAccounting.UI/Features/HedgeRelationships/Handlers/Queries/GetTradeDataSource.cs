using DerivativeEDGE.HedgeAccounting.UI.Helpers;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public static class GetTradeDataSource
{
    public class Query : IRequest<Response>
    {
        public long ClientId { get; set; }
        public long? BankEntityId { get; set; }
    }

    public record Response(List<TradeDto> TradeData, bool IsSuccess, string ErrorMessage = null);

    private record ApiResponse(List<TradeDto> Result);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly HttpClient _httpClient;
        private readonly TokenProvider _tokenProvider;
        private readonly ILogger<Handler> _logger;

        public Handler(IHttpClientFactory factory, TokenProvider tokenProvider, ILogger<Handler> logger)
        {
            _httpClient = factory.CreateClient("SwapzillaTrade");
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var apiUrl = $"Trade/DataSource?bankEntityId={request.BankEntityId}&clientId={request.ClientId}";
            _logger.LogInformation("Calling API: {ApiUrl}", apiUrl);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken) }
            };

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Response Body: {ResponseContent}", LoggingSanitizer.Sanitize(content));

                if (!response.IsSuccessStatusCode)
                {
                    return ErrorLoggingHelper.LogAndReturnError(
                        _logger,
                        $"API request failed: {response.StatusCode}",
                        content,
                        msg => new Response(new(), false, msg));
                }

                var result = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Result == null)
                {
                    return ErrorLoggingHelper.LogAndReturnError(
                        _logger,
                        "API returned null or invalid trade data.",
                        content,
                        msg => new Response(new(), false, msg));
                }

                _logger.LogInformation("Retrieved {Count} trade items.", LoggingSanitizer.Sanitize(result.Result.Count.ToString()));
                return new Response(result.Result, true);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                var error = ex is TaskCanceledException ? "Request timeout occurred" : "Network error occurred while fetching trade data";
                _logger.LogError(ex, error);
                return new Response(new(), false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching trade data");
                return new Response(new(), false, "An unexpected error occurred");
            }
        }
    }
}
