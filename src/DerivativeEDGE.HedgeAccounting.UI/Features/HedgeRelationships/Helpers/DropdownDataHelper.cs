namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Helpers;

public static class DropdownDataHelper
{
    private static readonly Dictionary<string, List<DropdownModel>> DropdownCache = new(StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<DropdownModel> GetDropdownDatasource(string dataSet)
    {
        if (string.IsNullOrWhiteSpace(dataSet))
        {
            throw new ArgumentException("Data set must be provided.", nameof(dataSet));
        }

        if (DropdownCache.TryGetValue(dataSet, out var cached))
        {
            return cached;
        }

        List<DropdownModel> data = dataSet.ToLowerInvariant() switch
        {
            "intrinsicmethod" => GetIntrinsicMethodOptions(),
            "hedgingobjective" => GetHedgingObjectiveOptions(),
            "hedgedrisk" => GetHedgedRiskOptions(),
            "hedgeddirection" => GetHedgedDirectionOptions(),
            "hedgetype" => GetHedgeTypeOptions(),
            "contractualrate" => GetContractualRateOptions(),
            "hedgeditemtype" => GetHedgedItemTypeOptions(),
            "hedgeditem" => GetHedgedItemOptions(),
            "standard" => GetStandardOptions(),
            "fairvaluemethod" => GetFairValueMethodOptions(),
            _ => throw new ArgumentException($"Invalid data set: {dataSet}", nameof(dataSet))
        };

        DropdownCache[dataSet] = data;
        return data;
    }

    private static List<DropdownModel> GetIntrinsicMethodOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "ForwardMethod", Text = "Forward Method" },
        new() { Value = "SpotMethod", Text = "Spot Method" }
    };

    private static List<DropdownModel> GetHedgingObjectiveOptions() => new()
    {
        new() { ID = 1, Text = "None" },
        new() { ID = 2, Text = "1M Contractual Rate" },
        new() { ID = 3, Text = "1M FHLB" },
        new() { ID = 4, Text = "1M Purchased Option" },
        new() { ID = 5, Text = "3M Contractual Rate" },
        new() { ID = 6, Text = "3M FHLB" },
        new() { ID = 7, Text = "3M Purchased Option" },
        new() { ID = 8, Text = "Bond Long Haul" },
        new() { ID = 9, Text = "Bond Short Cut" },
        new() { ID = 10, Text = "Loan Long Haul" },
        new() { ID = 11, Text = "Loan Short Cut" },
        new() { ID = 12, Text = "TruPs" },
        new() { ID = 13, Text = "Last of Layer" },
        new() { ID = 14, Text = "Contractually Specified Rate Purchased Option" },
        new() { ID = 15, Text = "Net Investment Hedge - Forward Rate Method" }
    };

    private static List<DropdownModel> GetHedgedRiskOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "InterestRate", Text = "Interest Rate" },
        new() { Value = "ForeignExchange", Text = "Foreign Exchange" }
    };

    private static List<DropdownModel> GetHedgedDirectionOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "All", Text = "All" },
        new() { Value = "LimitDownside", Text = "Limit Downside" },
        new() { Value = "LimitUpside", Text = "Limit Upside" },
        new() { Value = "Corridor", Text = "Corridor" },
    };

    private static List<DropdownModel> GetHedgeTypeOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "CashFlow", Text = "Cash Flow" },
        new() { Value = "FairValue", Text = "Fair Value" }
    };

    private static List<DropdownModel> GetContractualRateOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "LIBORUSDBBA1M", Text = "LIBOR-USD-BBA 1M" },
        new() { Value = "LIBORUSDBBA3M", Text = "LIBOR-USD-BBA 3M" },
        new() { Value = "LIBORUSDBBA6M", Text = "LIBOR-USD-BBA 6M" },
        new() { Value = "SIFMA", Text = "SIFMA" },
        new() { Value = "OIS", Text = "OIS" },
        new() { Value = "Prime", Text = "USD-PRIME.H15" },
        new() { Value = "SOFR", Text = "SOFR" },
        new() { Value = "Other", Text = "Other" },
    };

    private static List<DropdownModel> GetHedgedItemTypeOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "Existing", Text = "Existing" },
        new() { Value = "Forecasted", Text = "Forecasted" }
    };

    private static List<DropdownModel> GetHedgedItemOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "Asset", Text = "Asset" },
        new() { Value = "Liability", Text = "Liability" }
    };

    private static List<DropdownModel> GetStandardOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "ASC815", Text = "ASC815" }
    };

    private static List<DropdownModel> GetFairValueMethodOptions() => new()
    {
        new() { Value = "None", Text = "None" },
        new() { Value = "BenchmarkOnly", Text = "Benchmark Only" },
        new() { Value = "AllCashFlows", Text = "All Cash Flows" }
    };
}