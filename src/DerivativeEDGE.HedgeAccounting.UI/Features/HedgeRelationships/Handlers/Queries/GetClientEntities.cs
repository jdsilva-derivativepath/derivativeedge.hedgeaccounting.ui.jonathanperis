namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;

public sealed class GetClientEntities
{
    public sealed record Query(long? ClientId) : IRequest<Response>;

    public sealed class Response(IEnumerable<Entity> entitieList)
    {
        public List<Entity> Entities { get; } = entitieList.ToList();
    }

    public sealed class Handler(IIdentityClient identityClient, IConfiguration configuration) :
         IRequestHandler<Query, Response>
    {
        private readonly IIdentityClient _identityClient = identityClient;
        private readonly string _apiKey = configuration[ConfigurationKeys.IdentityApiKey]!;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var data = await _identityClient.GetEntitiesAsync(request.ClientId, _apiKey) ?? new List<Entity>(); // Assuming RawEntity is the original type

            var entityList = data
                .Where(e => e.ClientLongName != null) // Ensuring non-null values before filtering
                .Select(e => new Entity
                {
                    EntityId = e.EntityId,
                    ClientId = e.ClientId,
                    ClientLongName = e.ClientLongName,
                    ClientShortName = e.ClientShortName,
                    EntityLongName = e.EntityLongName,
                    EntityShortName = e.EntityShortName,
                    City = e.City,
                    State = e.State,
                    Lei = e.Lei,
                    DtccId = e.DtccId,
                    Status = e.Status,
                    FunctionalCurrency = e.FunctionalCurrency,
                    IsActive = e.IsActive
                })
                .DistinctBy(c => c.ClientLongName) // Ensuring distinct filtering logic is intentional
                .OrderBy(c => c.ClientLongName) // Ordering by ClientLongName
                .ToList(); // Final execution

            return new Response(entityList);
        }

    }
}