using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

public static class DesignationRequirementsValidator
{
    public static List<string> Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        var errors = new List<string>();

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return errors;
        }

        // Hedged/Hedging Items
        if ((model.HedgedItems?.Count ?? 0) == 0 || (model.HedgingItems?.Count ?? 0) == 0)
        {
            errors.Add("Hedged and Hedging Items are required to generate inception package");
        }
        
        // Report Currency
        if (string.IsNullOrEmpty(model.ReportCurrency))
        {
            errors.Add("Report currency is required");
        }
        
        // Effectiveness Methods
        if (model.ProspectiveEffectivenessMethodID == null || model.ProspectiveEffectivenessMethodID == 0)
        {
            errors.Add("Prospective Effectiveness Method is required");
        }
        
        // Retrospective Methods
        if (model.RetrospectiveEffectivenessMethodID == null || model.RetrospectiveEffectivenessMethodID == 0)
        {
            errors.Add("Retrospective Effectiveness Method is required");
        }

        // Hedged Items must be in HA status
        if (model.HedgedItems != null && model.HedgedItems.Any(item => item.ItemStatus != DerivativeEDGEDomainEntitiesEnumsTradeStatus.HA))
        {
            errors.Add("Hedged items must be in HA status.");
        }

        // Hedging Items must be in Validated status
        if (model.HedgingItems != null && model.HedgingItems.Any(item => item.ItemStatus != DerivativeEDGEDomainEntitiesEnumsTradeStatus.Validated))
        {
            errors.Add("Hedging items must be in validated status.");
        }

        DateTime dedesignationDate;
        DateTime designationDate;

        // DedesignationDate > DesignationDate
        if (!string.IsNullOrEmpty(model.DedesignationDate))
        {           
            if (DateTime.TryParse(model.DedesignationDate, out dedesignationDate) &&
                DateTime.TryParse(model.DesignationDate, out designationDate) &&
                dedesignationDate <= designationDate)
            {
                errors.Add("Dedesignation Date must be later than Designation Date");
            }
        }

        // HedgeType and related fields
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue)
        {
            if (model.FairValueMethod == null)
            {
                errors.Add("Fair Value Method must be specified");
            }

            if (model.Benchmark == null)
            {
                errors.Add("Benchmark index must be specified");
            }
        }
        else if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow && 
                 model.HedgeRiskType != DerivativeEDGEHAEntityEnumHedgeRiskType.ForeignExchange &&
                 model.Benchmark == null)
        {
            errors.Add("Contractual Rate must be specified");
        }

        // Hedged Item Type and AssetLiability
        if (model.HedgedItemType == null)
        {
            errors.Add("Hedged Item Type must be specified");
        }

        if (model.AssetLiability == null)
        {
            errors.Add("Hedged Item must be specified");
        }

        // DesignationDate not in the future
        if (DateTime.TryParse(model.DesignationDate, out designationDate) && 
            designationDate.Date > DateTime.Today)
        {
            errors.Add("Designation Date must equal or earlier than the current date");
        }

        ValidateOptionHedgeItems(model, errors);

        // CashFlow/OffMarket requires Amortization schedule
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow &&
            model.OffMarket &&
            !(model.HedgeRelationshipOptionTimeValueAmorts?.Any(a => a.OptionTimeValueAmortType == DerivativeEDGEHAEntityEnumOptionTimeValueAmortType.Amortization) ?? false))
        {
            errors.Add("An Amortization Schedule is required for an Off-Market Hedge Relationship.");
        }

        return errors;
    }

    private static void ValidateOptionHedgeItems(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model, List<string> errors)
    {
        if (model == null || !model.IsAnOptionHedge)
        {
            return;
        }

        var validHedgedTypes = new[] { "CapFloor", "Collar", "Corridor", "Swaption", "SwapWithOption", "Debt" };
        var validHedgingTypes = new[] { "CapFloor", "Collar", "Corridor", "Swaption", "SwapWithOption" };

        bool hasInvalidHedgedItem = model.HedgedItems != null &&
            model.HedgedItems.Any(item => !validHedgedTypes.Contains(item.SecurityType.ToString()));
        bool hasInvalidHedgingItem = model.HedgingItems != null &&
            model.HedgingItems.Any(item => !validHedgingTypes.Contains(item.SecurityType.ToString()));

        if (hasInvalidHedgedItem && hasInvalidHedgingItem)
        {
            errors.Add(
                "When 'Hedge is an Option' is checked, Trades participating as a Hedged Item or Hedging Item must be one of the following:" +
                "\n\tHedged Item - Cap/Floor, Collar, Corridor, Swaption, Swap with Option, Debt" +
                "\n\tHedging Item - Cap/Floor, Collar, Corridor, Swaption, Swap with Option"
            );
        }
        else if (hasInvalidHedgedItem && !hasInvalidHedgingItem)
        {
            errors.Add(
                "When 'Hedge is an Option' is checked, Trade participating as a Hedged Item must be one of the following:" +
                "\n\tCap/Floor, Collar, Corridor, Swaption, Swap with Option, Debt"
            );
        }
        else if (!hasInvalidHedgedItem && hasInvalidHedgingItem)
        {
            errors.Add(
                "When 'Hedge is an Option' is checked, Trade participating as a Hedging Item must be one of the following:" +
                "\n\tCap/Floor, Collar, Corridor, Swaption, Swap with Option"
            );
        }
    }
}
