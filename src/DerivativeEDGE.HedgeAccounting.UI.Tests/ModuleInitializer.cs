using ImageMagick;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VerifyTests.AngleSharp;

namespace DerivativeEDGE.HedgeAccounting.UI.Tests;

public static partial class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyPlaywright.Initialize();
        VerifyImageMagick.Initialize();

        #region scrubbers

        // remove some noise from the html snapshot
        VerifierSettings.ScrubEmptyLines();
        VerifierSettings.ScrubLinesWithReplace(s =>
        {
            var scrubbed = BlazorCommentRegex().Replace(s, string.Empty);
            if (string.IsNullOrWhiteSpace(scrubbed))
            {
                return null;
            }
            var scrubbedGuid = BlazorGuidAttributeRegex().Replace(scrubbed, "''");
            var scrubbedHyphenAttributes = BlazorHyphenAttributeRegex().Replace(scrubbedGuid, string.Empty);
            return BlazorUnderscoreAttributeRegex().Replace(scrubbedHyphenAttributes, string.Empty);
        });
        HtmlPrettyPrint.All();
        VerifierSettings.ScrubLinesContaining("<script src=\"_framework/dotnet.");

        #endregion

        VerifyImageMagick.RegisterComparers(
            threshold: 0.03,
            metric: ErrorMetric.MeanAbsolute);
    }

    [GeneratedRegex("<!--Blazor[^>]*>")]
    private static partial Regex BlazorCommentRegex();

    [GeneratedRegex("(?:[A-Za-z]\\-\\w+)=")]
    private static partial Regex BlazorHyphenAttributeRegex();

    [GeneratedRegex("(_[A-Za-z][A-Za-z]_[0-9[A-Za-z-]*)")]
    private static partial Regex BlazorUnderscoreAttributeRegex();

    [GeneratedRegex("[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?")]
    private static partial Regex BlazorGuidAttributeRegex();
}
