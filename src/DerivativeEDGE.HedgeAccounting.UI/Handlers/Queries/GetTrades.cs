namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Handler.Queries;

public sealed class GetTrades
{
    public sealed class Response
    {
        public IEnumerable<ClientTrade> Trades { get; set; }
        public Response(IEnumerable<ClientTrade> trades)
        {
            Trades = trades;
        }
    }

    public sealed class Query : IRequest<Response>
    {
    }

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly ITradeClient _tradeServiceClient;
        private readonly IMapper _mapper;
        public Handler(ITradeClient tradeServiceClient, IMapper mapper)
        {
            _tradeServiceClient = tradeServiceClient;
            _mapper = mapper;
        }
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var trades = await _tradeServiceClient.GetTrades();
            var tradesDto = _mapper.Map<IEnumerable<Trade>, IEnumerable<ClientTrade>>(trades);
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
