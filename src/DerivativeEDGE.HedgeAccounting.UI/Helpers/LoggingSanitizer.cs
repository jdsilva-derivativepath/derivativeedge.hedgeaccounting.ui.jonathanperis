namespace DerivativeEDGE.HedgeAccounting.UI.Helpers;
public static class LoggingSanitizer
{
    /// <summary>
    /// Replaces newlines to prevent log injection or log forging vulnerabilities.
    /// </summary>
    public static string Sanitize(string? input) =>
        input?.Replace("\r", "\\r").Replace("\n", "\\n") ?? "None";
}