public sealed class GetClients
{
    public sealed class Query : IRequest<Response> { }

    public sealed class Response
    {
        public List<Client> Clients { get; }

        public Response(IEnumerable<Client> clients)
        {
            Clients = [.. clients];
        }
    }

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly IIdentityClient _client;
        private readonly string _apiKey;
        private readonly TimeSpan _cacheExpiryTime;
        private readonly IAppCache _appCache;

        private const string CacheKey = "GetClientList";

        public Handler(IIdentityClient client, IAppCache appCache, IConfiguration configuration)
        {
            _client = client;
            _appCache = appCache;
            _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;
            _cacheExpiryTime = TimeSpan.FromMinutes(
                configuration.GetValue<int?>("CacheExpiryMinutes") ?? 10
            );
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            async Task<List<Client>> FetchClients()
            {
                var clients = await _client.GetClientsAsync(_apiKey);
                return [.. clients
                    .Select(c => new Client
                    {
                        ClientName = c.ClientName ?? string.Empty,
                        ClientId = c.ClientId
                    })
                    .Where(c => c.ClientId != null)
                    .DistinctBy(c => c.ClientId)
                    .OrderBy(c => c.ClientName)];
            }

            var clientList = await _appCache.GetOrAdd(CacheKey, FetchClients, _cacheExpiryTime);
            return new Response(clientList);
        }
    }
}
