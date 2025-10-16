namespace DerivativeEDGE.HedgeAccounting.UI.Helpers;

public static class ErrorLoggingHelper
{
    public static TResponse LogAndReturnError<TResponse>(
        ILogger logger,
        string message,
        string? content,
        Func<string, TResponse> createResponse)
    {
        logger.LogError("{Message} | Details: {Content}",
            LoggingSanitizer.Sanitize(message),
            LoggingSanitizer.Sanitize(content));

        return createResponse(message);
    }
}

