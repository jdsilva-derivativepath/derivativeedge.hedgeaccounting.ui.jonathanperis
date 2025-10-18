namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Handler.Queries;

public sealed class GetTrades
{
    public sealed class Response(IEnumerable<ClientTrade> trades)
    {
        public IEnumerable<ClientTrade> Trades { get; set; } = trades;
    }

    public sealed class Query : IRequest<Response>
    {
    }

    public class Handler(ITradeClient tradeServiceClient, IMapper mapper) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var trades = await tradeServiceClient.GetTrades();
            var tradesDto = mapper.Map<IEnumerable<Trade>, IEnumerable<ClientTrade>>(trades);
            return new Response(tradesDto);
        }
    }

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Trade, ClientTrade>();
        }
    }
}
