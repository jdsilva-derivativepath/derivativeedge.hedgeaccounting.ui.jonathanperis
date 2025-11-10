namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Helpers;

public static class StringHelpers
{
    /// <summary>
    /// Adds spaces before capital letters to improve readability.
    /// Works with enums, PascalCase, and camelCase. Returns an empty string for null or empty values.
    /// </summary>
    public static string FormatWithSpaces(object input)
    {
        if (input == null) return string.Empty;

        var text = input.ToString();
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"(?<=[a-z0-9])(?=[A-Z])",
            " "
        );
    }
}
