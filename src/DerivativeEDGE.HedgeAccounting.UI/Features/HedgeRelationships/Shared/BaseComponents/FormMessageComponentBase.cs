namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Shared.BaseComponents;

public class FormMessageComponentBase : ComponentBase
{
    protected MessageSeverity MessageSeverityType { get; private set; } = MessageSeverity.Error;
    protected string MessageTitle { get; private set; }
    protected string MessageBody { get; private set; }
    protected bool IsMessageVisible { get; private set; }

    protected void ShowFormMessage(FormMessageType type)
    {
        var (severity, title, body) = GetMessageContent(type);

        MessageSeverityType = severity;
        MessageTitle = title;
        MessageBody = body;
        IsMessageVisible = true;
    }

    protected void ShowCustomMessage(string title, string body, MessageSeverity severity = MessageSeverity.Info)
    {
        MessageTitle = title;
        MessageBody = body;
        MessageSeverityType = severity;
        IsMessageVisible = true;
    }

    protected void ShowCustomMessage(string title, string body, FormMessageType type)
    {
        ShowCustomMessage(title, body, MapFormMessageTypeToSeverity(type));
    }

    protected void HideFormMessage()
    {
        IsMessageVisible = false;
    }

    private (MessageSeverity Severity, string Title, string Body) GetMessageContent(FormMessageType type)
    {
        return type switch
        {
            FormMessageType.ValidationError => (
                MessageSeverity.Error,
                "Some fixes are needed",
                "Please update the highlighted fields to proceed."
            ),
            FormMessageType.ExceptionError => (
                MessageSeverity.Error,
                "User creation failed",
                "There was a system error while creating this user. Please try again, or contact support if the problem continues."
            ),
            FormMessageType.UnknownError => (
                MessageSeverity.Error,
                "An error occurred",
                "An unexpected error occurred. Please try again later."
            ),
            FormMessageType.Warning => (
                MessageSeverity.Warning,
                "Warning",
                "This is a warning message."
            ),
            _ => (
                MessageSeverity.Info,
                "Notice",
                "An informational message."
            )
        };
    }

    protected static MessageSeverity MapFormMessageTypeToSeverity(FormMessageType type)
    {
        return type switch
        {
            FormMessageType.ValidationError => MessageSeverity.Error,
            FormMessageType.ExceptionError => MessageSeverity.Error,
            FormMessageType.UnknownError => MessageSeverity.Error,
            FormMessageType.Warning => MessageSeverity.Warning,
            _ => MessageSeverity.Info
        };
    }
}