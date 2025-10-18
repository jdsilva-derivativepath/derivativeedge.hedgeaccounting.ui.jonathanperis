namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Handler.Queries;

public sealed class GetClientNames
{
    public sealed record Query(List<long> ClientIds) : IRequest<Response>;

    public sealed class Response(IEnumerable<ClientName> clientNames)
    {
        public List<ClientName> ClientNames { get; } = [.. clientNames];
    }

    public sealed class Handler(IIdentityClient client, IAppCache appCache, IConfiguration configuration) : IRequestHandler<Query, Response>
    {
        private readonly string _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;
        private readonly TimeSpan _cacheExpiryTime = new(0, 1, 0);

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var key = ToCacheKey(request.ClientIds, nameof(Handle));
            async Task<List<ClientName>> localGet()
            {

                var clients = await client.GetClientsByIdsAsync(request.ClientIds, _apiKey);
                return [.. clients.Select(client =>
                    new ClientName()
                    {
                        Name = client.ClientName ?? string.Empty,
                        ShortName = client.ClientShortName ?? string.Empty,
                        Id = client.ClientId
                    })];
            }
            var clientNames = await appCache.GetOrAdd(key, localGet, _cacheExpiryTime);
            return new Response(clientNames);
        }

        private static string ToCacheKey(ICollection<long> ids, string methodName)
        {
            var sortedIds = ids.ToList();
            sortedIds.Sort();
            var joinedString = string.Join('_', sortedIds.Select(i => i.ToString()));
            var hash = joinedString.GetHashCode();
            return $"{methodName}_{hash}";
        }
    }
}