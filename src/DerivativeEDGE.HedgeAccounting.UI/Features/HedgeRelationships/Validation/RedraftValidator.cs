using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

/// <summary>
/// Validates requirements for redrafting a hedge relationship.
/// A hedge relationship can only be redrafted if it is in Designated or Dedesignated state.
/// </summary>
public static class RedraftValidator
{
    public static List<string> Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Check hedge state - can only redraft from Designated or Dedesignated
        if (model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Designated &&
            model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
        {
            errors.Add($"Cannot redraft hedge relationship in '{model.HedgeState}' state. Redraft is only available for Designated or De-designated relationships.");
        }

        // Ensure hedge relationship ID is valid
        if (model.ID <= 0)
        {
            errors.Add("Invalid hedge relationship ID");
        }

        return errors;
    }
}
