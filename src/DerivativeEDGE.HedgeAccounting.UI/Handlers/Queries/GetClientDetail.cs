namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetClientDetail
{
    public sealed record Query(long ClientId) : IRequest<Response>;

    public sealed class Response(ClientDetail clientDetail)
    {
        public ClientDetail ClientDetail { get; } = clientDetail ?? new();
    }

    public sealed class Handler(IIdentityClient identityClient, IConfiguration configuration) : 
        IRequestHandler<Query, Response>
    {
        private readonly IIdentityClient _identityClient = identityClient;
        private readonly string _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var client = await _identityClient.GetClientByIdAsync(request.ClientId, _apiKey, cancellationToken);
            var clientDetail = new ClientDetail()
                {
                    Name = client?.ClientName ?? string.Empty,
                    ShortName = client?.ClientShortName ?? string.Empty,
                    Id = client?.ClientId ?? 0,
                    ContractType = client?.ContractType ?? ""
                };
            return new Response(clientDetail);
        }
    }
}
