namespace DerivativeEDGE.HedgeAccounting.UI.Handlers.Behaviors;

public sealed class LogExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{nameOfRequest}", typeof(TRequest).Name);
            throw;
        }
    }
}
