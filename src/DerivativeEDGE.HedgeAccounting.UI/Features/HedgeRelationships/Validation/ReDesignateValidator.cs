namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

public static class ReDesignateValidator
{
    public static List<string> Validate(
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model,
        DateTime redesignationDate)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Check hedge state - can only re-designate from Designated state
        if (model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Designated)
        {
            errors.Add($"Cannot re-designate hedge relationship in '{model.HedgeState}' state. Re-designation is only available for Designated relationships.");
        }

        // Check hedge type - re-designation is only for Cash Flow hedges (from legacy code)
        if (model.HedgeType != DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
        {
            errors.Add($"Re-designation is only available for Cash Flow hedge relationships. Current hedge type is '{model.HedgeType}'.");
        }

        // Ensure hedge relationship ID is valid
        if (model.ID <= 0)
        {
            errors.Add("Invalid hedge relationship ID");
        }

        // Validate redesignation date
        if (redesignationDate == default)
        {
            errors.Add("Re-designation date is required");
        }

        // Redesignation date should not be in the future
        if (redesignationDate.Date > DateTime.Today)
        {
            errors.Add("Re-designation date cannot be in the future");
        }

        // Redesignation date must be after designation date
        if (!string.IsNullOrEmpty(model.DesignationDate))
        {
            if (DateTime.TryParse(model.DesignationDate, out var designationDate))
            {
                if (redesignationDate.Date <= designationDate.Date)
                {
                    errors.Add("Re-designation date must be later than designation date");
                }
            }
        }

        // If there's a dedesignation date, redesignation date must be after it
        if (!string.IsNullOrEmpty(model.DedesignationDate))
        {
            if (DateTime.TryParse(model.DedesignationDate, out var dedesignationDate))
            {
                if (redesignationDate.Date <= dedesignationDate.Date)
                {
                    errors.Add("Re-designation date must be later than de-designation date");
                }
            }
        }

        return errors;
    }
}
