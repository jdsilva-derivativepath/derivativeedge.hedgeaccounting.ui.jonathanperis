namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Helpers;
public static class DateHelpers
{
    /// <summary>
    /// Tries to parse a date string and returns it in the specified format.
    /// If parsing fails, returns the original value or an empty string.
    /// </summary>
    public static string FormatDateOrOriginal(string dateString, string format = "MM/dd/yyyy")
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return string.Empty;

        return DateTime.TryParse(dateString, out var parsed)
            ? parsed.ToString(format)
            : dateString;
    }
}

