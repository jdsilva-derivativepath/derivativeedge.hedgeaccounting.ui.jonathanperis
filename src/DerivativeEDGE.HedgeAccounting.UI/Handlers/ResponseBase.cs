namespace DerivativeEDGE.HedgeAccounting.UI.Handlers;
public class ResponseBase
{
    public bool HasError { get; init; }
    public bool TaskCanceled { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string StackTrace { get; init; } = string.Empty;
    public ResponseBase() { }

    public ResponseBase(Exception exception)
    {
        TaskCanceled = exception is TaskCanceledException or OperationCanceledException;
        HasError = true;
        ErrorMessage = exception.Message;
        StackTrace = exception.StackTrace ?? string.Empty;
    }
}
