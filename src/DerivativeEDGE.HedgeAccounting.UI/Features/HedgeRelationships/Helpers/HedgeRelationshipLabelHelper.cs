using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Helpers;

public static class HedgeRelationshipLabelHelper
{
    public static string GetBenchMarkLabel(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship)
    {
        if (HedgeRelationship?.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.InterestRate && HedgeRelationship?.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
        {
            return "Contractual Rate";
        }
            
        return "Benchmark";
    }

    public static IEnumerable<DropdownModel> FilterBenchmarkList(IEnumerable<DropdownModel> allBenchmarks, string hedgeType)
    {
        var notCFBenchmarks = new[] { "FFUTFDTR", "FHLBTopeka", "USDTBILL4WH15" };
        var notFVBenchmarks = new[] { "FFUTFDTR", "FHLBTopeka", "USDTBILL4WH15", "Other", "Prime" };

        if (hedgeType == "CashFlow")
        {
            return allBenchmarks.Where(v => !notCFBenchmarks.Contains(v.Value));
        }  
        else
        {
            return allBenchmarks.Where(v => !notFVBenchmarks.Contains(v.Value));
        }
    }
}
