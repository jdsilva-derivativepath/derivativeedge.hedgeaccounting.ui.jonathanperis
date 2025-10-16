using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

public static class RedraftRequirementsValidator
{
    public static List<string> Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Redraft is only allowed from Designated or Dedesignated states
        if (model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Designated &&
            model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
        {
            errors.Add("Redraft is only allowed for Designated or Dedesignated hedge relationships");
        }

        return errors;
    }
}
