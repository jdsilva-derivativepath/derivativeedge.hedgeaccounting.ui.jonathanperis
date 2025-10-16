namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Behaviors;

public sealed class LogExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public LogExceptionBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{nameOfRequest}", typeof(TRequest).Name);
            throw;
        }
    }
}
