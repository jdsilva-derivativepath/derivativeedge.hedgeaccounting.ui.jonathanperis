using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;
public static class RegressionRequirementsValidator
{
    public static List<string> Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM hedgeRelationship)
    {
        var errors = new List<string>();

        if (hedgeRelationship == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Mimic JavaScript validation logic
        if (hedgeRelationship.ProspectiveEffectivenessMethodID == null || hedgeRelationship.ProspectiveEffectivenessMethodID == 0)
        {
            errors.Add("Prospective Effectiveness Method is required");
        }

        if (hedgeRelationship.RetrospectiveEffectivenessMethodID == null || hedgeRelationship.RetrospectiveEffectivenessMethodID == 0)
        {
            errors.Add("Retrospective Effectiveness Method is required");
        }

        if ((hedgeRelationship.HedgedItems?.Count ?? 0) == 0 || (hedgeRelationship.HedgingItems?.Count ?? 0) == 0)
        {
            errors.Add("Hedged and Hedging Items are required to test");
        }

        if (string.IsNullOrEmpty(hedgeRelationship.ReportCurrency) || hedgeRelationship.ReportCurrency == "None")
        {
            errors.Add("Report currency is required");
        }

        return errors;
    }
}