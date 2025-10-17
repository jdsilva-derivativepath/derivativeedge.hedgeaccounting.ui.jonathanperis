namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

/// <summary>
/// Validates requirements for de-designating a hedge relationship.
/// </summary>
public static class DeDesignateValidator
{
    public static List<string> Validate(
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model,
        DateTime dedesignationDate,
        int dedesignationReason)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Check hedge state - can only de-designate from Designated state
        if (model.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Designated)
        {
            errors.Add($"Cannot de-designate hedge relationship in '{model.HedgeState}' state. De-designation is only available for Designated relationships.");
        }

        // Ensure hedge relationship ID is valid
        if (model.ID <= 0)
        {
            errors.Add("Invalid hedge relationship ID");
        }

        // Validate dedesignation date
        if (dedesignationDate == default)
        {
            errors.Add("De-designation date is required");
        }

        // Dedesignation date should not be in the future
        if (dedesignationDate.Date > DateTime.Today)
        {
            errors.Add("De-designation date cannot be in the future");
        }

        // Dedesignation date must be after designation date
        if (!string.IsNullOrEmpty(model.DesignationDate))
        {
            if (DateTime.TryParse(model.DesignationDate, out var designationDate))
            {
                if (dedesignationDate.Date <= designationDate.Date)
                {
                    errors.Add("De-designation date must be later than designation date");
                }

                // Check if dedesignation is within 3 months of designation (warning from legacy code)
                var monthsDifference = (dedesignationDate.Year - designationDate.Year) * 12 + 
                                     dedesignationDate.Month - designationDate.Month;
                
                if (monthsDifference < 3)
                {
                    // This is a warning condition in legacy code - user should confirm
                    // Not adding as a blocking error, but UI should prompt for confirmation
                }
            }
        }

        // Validate dedesignation reason
        if (dedesignationReason <= 0)
        {
            errors.Add("De-designation reason is required");
        }

        return errors;
    }
}
