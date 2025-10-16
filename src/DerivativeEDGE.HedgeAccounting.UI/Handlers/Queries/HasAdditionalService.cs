namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class HasAdditionalService
{
    public sealed record Query(long ClientId, AddtlServices AdditionalService) : IRequest<Response>;

    public sealed class Response(bool hasAdditionalService)
    {
        public bool HasAdditionalService => hasAdditionalService;
    }

    public sealed class Handler(IIdentityClient identityClient, IConfiguration configuration) : 
        IRequestHandler<Query, Response>
    {
        private readonly IIdentityClient _identityClient = identityClient;
        private readonly string _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await _identityClient.GetClientByIdAsync(request.ClientId, _apiKey, cancellationToken);
            var addtlService = request.AdditionalService.ToString();
            var hasAdditionalService = !string.IsNullOrWhiteSpace(client.AdditionalServices) &&
                client.AdditionalServices.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(svc => string.Equals(svc, addtlService, StringComparison.InvariantCultureIgnoreCase));
            return new Response(hasAdditionalService);
        }
    }
}
