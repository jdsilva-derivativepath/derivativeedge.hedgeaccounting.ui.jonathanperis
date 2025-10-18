public sealed class GetClients
{
    public sealed class Query : IRequest<Response> { }

    public sealed class Response(IEnumerable<Client> clients)
    {
        public List<Client> Clients { get; } = [.. clients];
    }

    public sealed class Handler(IIdentityClient client, IAppCache appCache, IConfiguration configuration) : IRequestHandler<Query, Response>
    {
        private readonly string _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;
        private readonly TimeSpan _cacheExpiryTime = TimeSpan.FromMinutes(
                configuration.GetValue<int?>("CacheExpiryMinutes") ?? 10
            );
        private const string CacheKey = "GetClientList";

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            async Task<List<Client>> FetchClients()
            {
                var clients = await client.GetClientsAsync(_apiKey);
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

            var clientList = await appCache.GetOrAdd(CacheKey, FetchClients, _cacheExpiryTime);
            return new Response(clientList);
        }
    }
}
