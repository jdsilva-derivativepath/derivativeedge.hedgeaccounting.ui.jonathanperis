namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Validation;

public static class SaveHedgeRelationshipValidator
{
    /// <summary>
    /// Validates a hedge relationship before saving (Entity version).
    /// Returns a tuple: (isValid, errors, needsConfirmation, confirmationMessage)
    /// </summary>
    public static (bool IsValid, List<string> Errors, bool NeedsConfirmation, string ConfirmationMessage) 
        Validate(DerivativeEDGEHAEntityHedgeRelationship model)
    {
        var errors = new List<string>();
        var needsConfirmation = false;
        var confirmationMessage = string.Empty;

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return (false, errors, false, string.Empty);
        }

        // Dedesignation Date validation (legacy: lines 2170-2179)
        if (model.DedesignationDate.HasValue)
        {
            var designationDate = model.DesignationDate.Date; // Date portion only
            var dedesignationDate = model.DedesignationDate.Value.Date; // Date portion only

            if (dedesignationDate <= designationDate)
            {
                errors.Add("Dedesignation Date must be later than Designation Date");
            }
            else
            {
                // Check if dedesignation date is within 3 months of designation date
                var threeMonthsAfter = designationDate.AddMonths(3);
                if (dedesignationDate <= threeMonthsAfter)
                {
                    needsConfirmation = true;
                    confirmationMessage = "Dedesignation date should be 3 months after designation date. Are you sure you want to continue?";
                }
            }
        }

        // Designation Date validation (legacy: lines 2181-2183)
        if (model.DesignationDate.Date > DateTime.Today)
        {
            errors.Add("Designation Date must equal or earlier than the current date");
        }

        // Fair Value + Long Haul + Qualitative Assessment validation (legacy: lines 2185-2187)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue &&
            !model.Shortcut &&
            model.QualitativeAssessment)
        {
            errors.Add("Qualitative Assessment cannot be selected for a Fair Value Long Haul Hedge Relationship.");
        }

        // Fair Value + Shortcut + Portfolio Layer Method validation (legacy: lines 2189-2191)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue &&
            model.Shortcut &&
            model.PortfolioLayerMethod)
        {
            errors.Add("Portfolio Layer Method cannot be selected for a Fair Value Shortcut Hedge Relationship.");
        }

        // Hedging Instrument Structure validation (legacy: lines 2193-2195)
        if (model.HedgingInstrumentStructure == DerivativeEDGEHAEntityEnumHedgingInstrumentStructure.SingleInstrument &&
            model.HedgingItems != null &&
            model.HedgingItems.Count > 1)
        {
            errors.Add("There is more than one Hedging Instrument participating in the Hedge Relationship.  Either update the Hedging Instrument Structure or remove additional Hedging Instruments.");
        }

        // Option Premium validation (legacy: checkOptionPremium function)
        if (model.IsAnOptionHedge)
        {
            if (model.OptionPremium is < 0)
            {
                errors.Add("Option Premium must be greater than zero");
            }
        }

        // Option Hedge Items validation (legacy: checkOptionHedgeItems function)
        ValidateOptionHedgeItems(model, errors);

        var isValid = errors.Count == 0;
        return (isValid, errors, needsConfirmation, confirmationMessage);
    }

    /// <summary>
    /// Validates a hedge relationship before saving (ViewModel version).
    /// Returns a tuple: (isValid, errors, needsConfirmation, confirmationMessage)
    /// </summary>
    public static (bool IsValid, List<string> Errors, bool NeedsConfirmation, string ConfirmationMessage) 
        Validate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        var errors = new List<string>();
        var needsConfirmation = false;
        var confirmationMessage = string.Empty;

        if (model == null)
        {
            errors.Add("Hedge relationship data is not available");
            return (false, errors, false, string.Empty);
        }

        // Dedesignation Date validation (legacy: lines 2170-2179)
        if (!string.IsNullOrEmpty(model.DedesignationDate))
        {
            if (DateTime.TryParse(model.DesignationDate, out DateTime designationDate) &&
                DateTime.TryParse(model.DedesignationDate, out DateTime dedesignationDate))
            {
                if (dedesignationDate <= designationDate)
                {
                    errors.Add("Dedesignation Date must be later than Designation Date");
                }
                else
                {
                    // Check if dedesignation date is within 3 months of designation date
                    var threeMonthsAfter = designationDate.AddMonths(3);
                    if (dedesignationDate <= threeMonthsAfter)
                    {
                        needsConfirmation = true;
                        confirmationMessage = "Dedesignation date should be 3 months after designation date. Are you sure you want to continue?";
                    }
                }
            }
        }

        // Designation Date validation (legacy: lines 2181-2183)
        if (DateTime.TryParse(model.DesignationDate, out DateTime parsedDesignationDate) &&
            parsedDesignationDate.Date > DateTime.Today)
        {
            errors.Add("Designation Date must equal or earlier than the current date");
        }

        // Fair Value + Long Haul + Qualitative Assessment validation (legacy: lines 2185-2187)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue &&
            !model.Shortcut &&
            model.QualitativeAssessment)
        {
            errors.Add("Qualitative Assessment cannot be selected for a Fair Value Long Haul Hedge Relationship.");
        }

        // Fair Value + Shortcut + Portfolio Layer Method validation (legacy: lines 2189-2191)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue &&
            model.Shortcut &&
            model.PortfolioLayerMethod)
        {
            errors.Add("Portfolio Layer Method cannot be selected for a Fair Value Shortcut Hedge Relationship.");
        }

        // Hedging Instrument Structure validation (legacy: lines 2193-2195)
        if (model.HedgingInstrumentStructure == DerivativeEDGEHAEntityEnumHedgingInstrumentStructure.SingleInstrument &&
            model.HedgingItems != null &&
            model.HedgingItems.Count > 1)
        {
            errors.Add("There is more than one Hedging Instrument participating in the Hedge Relationship.  Either update the Hedging Instrument Structure or remove additional Hedging Instruments.");
        }

        // Option Premium validation (legacy: checkOptionPremium function)
        if (model.IsAnOptionHedge)
        {
            if (model.OptionPremium is < 0)
            {
                errors.Add("Option Premium must be greater than zero");
            }
        }

        // Option Hedge Items validation (legacy: checkOptionHedgeItems function)
        ValidateOptionHedgeItems(model, errors);

        var isValid = errors.Count == 0;
        return (isValid, errors, needsConfirmation, confirmationMessage);
    }

    /// <summary>
    /// Applies field cleanup and default values based on hedge type and other properties (Entity version).
    /// Matches legacy logic from $scope.submit() function (lines 2158-2208).
    /// </summary>
    public static void ApplyFieldCleanupAndDefaults(DerivativeEDGEHAEntityHedgeRelationship model)
    {
        if (model == null)
        {
            return;
        }

        // Delete effectiveness method IDs if they're 0 (legacy: lines 2158-2162)
        if (model.ProspectiveEffectivenessMethodID == 0)
        {
            model.ProspectiveEffectivenessMethodID = null;
        }

        if (model.RetrospectiveEffectivenessMethodID == 0)
        {
            model.RetrospectiveEffectivenessMethodID = null;
        }

        // Option Hedge logic (legacy: lines 2164-2166)
        if (model.IsAnOptionHedge)
        {
            model.OffMarket = false;
        }

        // Pre-Issuance Hedge logic (legacy: lines 2197-2199)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue ||
            model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
        {
            model.PreIssuanceHedge = false;
        }

        // Portfolio Layer Method logic (legacy: lines 2201-2203)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow ||
            model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
        {
            model.PortfolioLayerMethod = false;
        }

        // Benchmark/Contractual Rate/Exposure cleanup (legacy: setBenchmarkContractualRateExposure function)
        SetBenchmarkContractualRateExposure(model);
    }

    /// <summary>
    /// Applies field cleanup and default values based on hedge type and other properties (ViewModel version).
    /// Matches legacy logic from $scope.submit() function (lines 2158-2208).
    /// </summary>
    public static void ApplyFieldCleanupAndDefaults(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        if (model == null)
        {
            return;
        }

        // Delete effectiveness method IDs if they're 0 (legacy: lines 2158-2162)
        if (model.ProspectiveEffectivenessMethodID == 0)
        {
            model.ProspectiveEffectivenessMethodID = null;
        }

        if (model.RetrospectiveEffectivenessMethodID == 0)
        {
            model.RetrospectiveEffectivenessMethodID = null;
        }

        // Option Hedge logic (legacy: lines 2164-2166)
        if (model.IsAnOptionHedge)
        {
            model.OffMarket = false;
        }

        // Pre-Issuance Hedge logic (legacy: lines 2197-2199)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue ||
            model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
        {
            model.PreIssuanceHedge = false;
        }

        // Portfolio Layer Method logic (legacy: lines 2201-2203)
        if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow ||
            model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
        {
            model.PortfolioLayerMethod = false;
        }

        // Benchmark/Contractual Rate/Exposure cleanup (legacy: setBenchmarkContractualRateExposure function)
        SetBenchmarkContractualRateExposure(model);
    }

    private static void ValidateOptionHedgeItems(DerivativeEDGEHAEntityHedgeRelationship model, List<string> errors)
    {
        if (!model.IsAnOptionHedge)
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

    private static void ValidateOptionHedgeItems(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model, List<string> errors)
    {
        if (!model.IsAnOptionHedge)
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

    /// <summary>
    /// Sets benchmark, contractual rate, and exposure fields based on hedge risk type and hedge type (Entity version).
    /// Matches legacy logic from setBenchmarkContractualRateExposure() function.
    /// </summary>
    private static void SetBenchmarkContractualRateExposure(DerivativeEDGEHAEntityHedgeRelationship model)
    {
        if (model.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.ForeignExchange)
        {
            if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow ||
                model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue)
            {
                // Only safely reset Benchmark here to avoid casting issues; other fields omitted until enum nullability confirmed.
                model.Benchmark = DerivativeEDGEHAEntityEnumBenchmark.None;
            }
            else if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
            {
                model.Benchmark = DerivativeEDGEHAEntityEnumBenchmark.None;
            }
        }
        else if (model.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.InterestRate)
        {
            // Legacy clears HedgeExposure, ExposureCurrency, HedgeAccountingTreatment. Omitted due to enum nullability uncertainty.
        }
    }

    /// <summary>
    /// Sets benchmark, contractual rate, and exposure fields based on hedge risk type and hedge type (ViewModel version).
    /// Matches legacy logic from setBenchmarkContractualRateExposure() function.
    /// </summary>
    private static void SetBenchmarkContractualRateExposure(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM model)
    {
        if (model.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.ForeignExchange)
        {
            if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow ||
                model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.FairValue)
            {
                model.Benchmark = DerivativeEDGEHAEntityEnumBenchmark.None;
                model.ExposureCurrency = null; // viewmodel may be nullable/string; keep behavior
                model.HedgeAccountingTreatment = null;
            }
            else if (model.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.NetInvestment)
            {
                model.Benchmark = DerivativeEDGEHAEntityEnumBenchmark.None;
                model.HedgeExposure = null;
            }
        }
        else if (model.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.InterestRate)
        {
            model.HedgeExposure = null;
            model.ExposureCurrency = null;
            model.HedgeAccountingTreatment = null;
        }
    }
}
