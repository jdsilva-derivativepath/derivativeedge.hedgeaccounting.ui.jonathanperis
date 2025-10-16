using EdgeRole = DerivativeEDGE.Authorization.AuthClaims.EdgeRole;

namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Queries;

public sealed class GetAllowedBehavior
{
    public sealed class Query : IRequest<Response>
    {

    }

    public sealed class Response(AllowedBehavior allowedBehavior)
    {
        public AllowedBehavior AllowedBehavior { get; } = allowedBehavior;
    }

    public sealed class Handler(IUserAuthData userAuthData, IMediator mediator) : 
        IRequestHandler<Query, Response>
    {
        private readonly IUserAuthData _UserAuthData = userAuthData;
        private readonly IMediator _mediator = mediator;

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedBehavior = new AllowedBehavior()
            {
                ClientId = _UserAuthData.ClientId,
                IsDpiUser = _UserAuthData.IsDpiUser,
                UserId = _UserAuthData.UserId
            };

            var clientContractType = "";
            var query = new GetClientDetail.Query(allowedBehavior.ClientId);
            var response = await _mediator.Send(query, cancellationToken);
            if (response != null)
            {
                clientContractType = response.ClientDetail.ContractType;
            }

            if (_UserAuthData.HasRole(EdgeRole.HA) && allowedBehavior.IsDpiUser)
            {
                allowedBehavior.FullAccess = true;
            }
            else if (_UserAuthData.HasRole(EdgeRole.HA) && !allowedBehavior.IsDpiUser
                && StringConstants.ClientContractTypes.Contains(clientContractType))
            {
                allowedBehavior.FullAccess = true;
                allowedBehavior.IsSaasSwaSClient = true;
            }

            return new Response(allowedBehavior);
        }
    }
}
