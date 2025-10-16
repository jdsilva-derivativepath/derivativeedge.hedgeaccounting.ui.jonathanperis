namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetAllClientNames
{
    public sealed class Query : IRequest<Response>
    {

    }

    public sealed class Response(IEnumerable<ClientName> clientNames)
    {
        public List<ClientName> ClientNames { get; } = [.. clientNames];
    }

    public sealed class Handler(IIdentityClient client, IConfiguration configuration) : 
        IRequestHandler<Query, Response>
    {
        private readonly IIdentityClient _client = client;
        private readonly string _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var clients = await _client.GetClientsAsync(_apiKey);
            var addtlServiceHa = AddtlServices.HedgeAccounting.ToString();
            var clientNames = clients.Where(client => client.ClientId == 1 ||
                (!string.IsNullOrWhiteSpace(client.AdditionalServices) && 
                client.AdditionalServices.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(svc => string.Equals(svc, addtlServiceHa, StringComparison.InvariantCultureIgnoreCase))))
                .Select(client => new ClientName
                {
                    Name = client.ClientName ?? string.Empty,
                    ShortName = client.ClientShortName ?? string.Empty,
                    Id = client.ClientId
                }).DistinctBy(c => c.Id).OrderBy(c => c.Name).ToList();
            return new Response(clientNames);
        }
    }
}