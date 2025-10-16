using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

public static class ReDesignateRequirementsValidator
{
    public static List<string> Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Re-Designate is only allowed for Designated CashFlow hedges
        if (model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Designated)
        {
            errors.Add("Re-Designate is only allowed for Designated hedge relationships");
        }

        if (model.HedgeType != DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
        {
            errors.Add("Re-Designate is only allowed for CashFlow hedge types");
        }

        return errors;
    }
}
