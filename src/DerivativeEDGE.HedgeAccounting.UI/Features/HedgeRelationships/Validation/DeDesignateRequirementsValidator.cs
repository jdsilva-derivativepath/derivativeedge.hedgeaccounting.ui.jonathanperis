using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

public static class DeDesignateRequirementsValidator
{
    public static List<string> Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // De-Designate is only allowed from Designated state
        if (model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Designated)
        {
            errors.Add("De-Designate is only allowed for Designated hedge relationships");
        }

        // Must have hedging items
        if (model.HedgingItems == null || model.HedgingItems.Count == 0)
        {
            errors.Add("Hedging Items are required for De-Designation");
        }

        return errors;
    }
}
