namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Helpers;

public static class HedgeRelationshipDataHelper
{
    #region HTML Validation Helpers
    public static bool IsHtmlWhitespaceOnly(string html)
    {
        // Regex pattern that matches only "empty" HTML content (tags or spaces)
        var pattern = @"^(?:</?p>|</?br\s*/?>|</?div>|</?span>|\s|&nbsp;)*$";
        return Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase);
    }

    public static bool IsNewHedgeDocumentTemplate(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM hedgeRelationship)
    {
        if (hedgeRelationship == null)
            return false;

        return string.IsNullOrWhiteSpace(hedgeRelationship.Objective)
            || IsHtmlWhitespaceOnly(hedgeRelationship.Objective);
    }
    #endregion

    #region Chart Data Generation
    public static List<ChartDataModel> GenerateEffectivenessChartData(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM hedgeRelationship)
    {
        if (hedgeRelationship?.HedgeRegressionBatches?.Count > 0)
        {
            // Filter batches: take first 8, exclude 'User' type unless HedgeState is 'Draft'
            var filteredBatches = hedgeRelationship.HedgeRegressionBatches
                .Take(8)
                .Where(batch =>
                    (batch.HedgeResultType != DerivativeEDGEHAEntityEnumHedgeResultType.User && hedgeRelationship.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Draft) ||
                    hedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft)
                .ToList();

            // Sort by ValueDate
            filteredBatches = [.. filteredBatches.OrderBy(batch => batch.ValueDate)];

            var chartData = new List<ChartDataModel>();
            var processedDates = new HashSet<string>();

            foreach (var batch in filteredBatches)
            {
                // Remove duplicates by ValueDate (same logic as original JS)
                if (!processedDates.Contains(batch.ValueDate))
                {
                    // Round to 2 decimal places (same as original JS: Math.round(value + 'e2') + 'e-2')
                    var slope = Math.Round(batch.Slope, 2);
                    var rSquared = Math.Round(batch.RSquared, 2);

                    chartData.Add(new ChartDataModel
                    {
                        Date = batch.ValueDate,
                        R2Value = rSquared,
                        Slope = slope
                    });

                    processedDates.Add(batch.ValueDate);
                }
            }

            return chartData;
        }
        else
        {
            // Fallback to static data converted to DateTime format
            return [];
        }
    }
    #endregion

    #region Dropdown Data Options
    public static List<HedgingInstrumentStructureOption> GetHedgingInstrumentStructureOptions() =>
    [
        new() { Value = HedgingInstrumentStructure.SingleInstrument, Text = HedgingInstrumentStructure.SingleInstrument.GetDescription() },
        new() { Value = HedgingInstrumentStructure.StructuredProduct, Text = HedgingInstrumentStructure.StructuredProduct.GetDescription() },
        new() { Value = HedgingInstrumentStructure.MultipleInstruments, Text = HedgingInstrumentStructure.MultipleInstruments.GetDescription() }
    ];

    public static List<FinancialCenterOption> GetFinancialCenterOptions() =>
    [
        new() { Value = FinancialCenter.BEBR, Text = FinancialCenter.BEBR.GetDescription() },
        new() { Value = FinancialCenter.ARBA, Text = FinancialCenter.ARBA.GetDescription() },
        new() { Value = FinancialCenter.ATVI, Text = FinancialCenter.ATVI.GetDescription() },
        new() { Value = FinancialCenter.AUME, Text = FinancialCenter.AUME.GetDescription() },
        new() { Value = FinancialCenter.AUSY, Text = FinancialCenter.AUSY.GetDescription() },
        new() { Value = FinancialCenter.BRSP, Text = FinancialCenter.BRSP.GetDescription() },
        new() { Value = FinancialCenter.CAMO, Text = FinancialCenter.CAMO.GetDescription() },
        new() { Value = FinancialCenter.CATO, Text = FinancialCenter.CATO.GetDescription() },
        new() { Value = FinancialCenter.CHGE, Text = FinancialCenter.CHGE.GetDescription() },
        new() { Value = FinancialCenter.SKBR, Text = FinancialCenter.SKBR.GetDescription() },
        new() { Value = FinancialCenter.CLSA, Text = FinancialCenter.CLSA.GetDescription() },
        new() { Value = FinancialCenter.CNBE, Text = FinancialCenter.CNBE.GetDescription() },
        new() { Value = FinancialCenter.CZPR, Text = FinancialCenter.CZPR.GetDescription() },
        new() { Value = FinancialCenter.DEFR, Text = FinancialCenter.DEFR.GetDescription() },
        new() { Value = FinancialCenter.DKCO, Text = FinancialCenter.DKCO.GetDescription() },
        new() { Value = FinancialCenter.EETA, Text = FinancialCenter.EETA.GetDescription() },
        new() { Value = FinancialCenter.ESMA, Text = FinancialCenter.ESMA.GetDescription() },
        new() { Value = FinancialCenter.FIHE, Text = FinancialCenter.FIHE.GetDescription() },
        new() { Value = FinancialCenter.FRPA, Text = FinancialCenter.FRPA.GetDescription() },
        new() { Value = FinancialCenter.GBLO, Text = FinancialCenter.GBLO.GetDescription() },
        new() { Value = FinancialCenter.GRAT, Text = FinancialCenter.GRAT.GetDescription() },
        new() { Value = FinancialCenter.HKHK, Text = FinancialCenter.HKHK.GetDescription() },
        new() { Value = FinancialCenter.HUBU, Text = FinancialCenter.HUBU.GetDescription() },
        new() { Value = FinancialCenter.IDJA, Text = FinancialCenter.IDJA.GetDescription() },
        new() { Value = FinancialCenter.IEDU, Text = FinancialCenter.IEDU.GetDescription() },
        new() { Value = FinancialCenter.ILTA, Text = FinancialCenter.ILTA.GetDescription() },
        new() { Value = FinancialCenter.ITMI, Text = FinancialCenter.ITMI.GetDescription() },
        new() { Value = FinancialCenter.ITRO, Text = FinancialCenter.ITRO.GetDescription() },
        new() { Value = FinancialCenter.JPTO, Text = FinancialCenter.JPTO.GetDescription() },
        new() { Value = FinancialCenter.KRSE, Text = FinancialCenter.KRSE.GetDescription() },
        new() { Value = FinancialCenter.LBBE, Text = FinancialCenter.LBBE.GetDescription() },
        new() { Value = FinancialCenter.LULU, Text = FinancialCenter.LULU.GetDescription() },
        new() { Value = FinancialCenter.MYKL, Text = FinancialCenter.MYKL.GetDescription() },
        new() { Value = FinancialCenter.MXMC, Text = FinancialCenter.MXMC.GetDescription() },
        new() { Value = FinancialCenter.NLAM, Text = FinancialCenter.NLAM.GetDescription() },
        new() { Value = FinancialCenter.NOOS, Text = FinancialCenter.NOOS.GetDescription() },
        new() { Value = FinancialCenter.NYFD, Text = FinancialCenter.NYFD.GetDescription() },
        new() { Value = FinancialCenter.NYSE, Text = FinancialCenter.NYSE.GetDescription() },
        new() { Value = FinancialCenter.NZAU, Text = FinancialCenter.NZAU.GetDescription() },
        new() { Value = FinancialCenter.NZWE, Text = FinancialCenter.NZWE.GetDescription() },
        new() { Value = FinancialCenter.PAPC, Text = FinancialCenter.PAPC.GetDescription() },
        new() { Value = FinancialCenter.PHMA, Text = FinancialCenter.PHMA.GetDescription() },
        new() { Value = FinancialCenter.PLWA, Text = FinancialCenter.PLWA.GetDescription() },
        new() { Value = FinancialCenter.PTLI, Text = FinancialCenter.PTLI.GetDescription() },
        new() { Value = FinancialCenter.RUMO, Text = FinancialCenter.RUMO.GetDescription() },
        new() { Value = FinancialCenter.SARI, Text = FinancialCenter.SARI.GetDescription() },
        new() { Value = FinancialCenter.SEST, Text = FinancialCenter.SEST.GetDescription() },
        new() { Value = FinancialCenter.SGSI, Text = FinancialCenter.SGSI.GetDescription() },
        new() { Value = FinancialCenter.THBA, Text = FinancialCenter.THBA.GetDescription() },
        new() { Value = FinancialCenter.TWTA, Text = FinancialCenter.TWTA.GetDescription() },
        new() { Value = FinancialCenter.TRAN, Text = FinancialCenter.TRAN.GetDescription() },
        new() { Value = FinancialCenter.USCH, Text = FinancialCenter.USCH.GetDescription() },
        new() { Value = FinancialCenter.USLA, Text = FinancialCenter.USLA.GetDescription() },
        new() { Value = FinancialCenter.USGS, Text = FinancialCenter.USGS.GetDescription() },
        new() { Value = FinancialCenter.USNY, Text = FinancialCenter.USNY.GetDescription() },
        new() { Value = FinancialCenter.ZAJO, Text = FinancialCenter.ZAJO.GetDescription() },
        new() { Value = FinancialCenter.CHZU, Text = FinancialCenter.CHZU.GetDescription() },
        new() { Value = FinancialCenter.EUTA, Text = FinancialCenter.EUTA.GetDescription() },
        new() { Value = FinancialCenter.INMU, Text = FinancialCenter.INMU.GetDescription() },
        new() { Value = FinancialCenter.PKKA, Text = FinancialCenter.PKKA.GetDescription() },
        new() { Value = FinancialCenter.RWKI, Text = FinancialCenter.RWKI.GetDescription() },
        new() { Value = FinancialCenter.COBG, Text = FinancialCenter.COBG.GetDescription() },
        new() { Value = FinancialCenter.VNHA, Text = FinancialCenter.VNHA.GetDescription() },
        new() { Value = FinancialCenter.CME, Text = FinancialCenter.CME.GetDescription()  },
        new() { Value = FinancialCenter.GBEDI, Text = FinancialCenter.GBEDI.GetDescription() },
        new() { Value = FinancialCenter.KYGEC, Text = FinancialCenter.KYGEC.GetDescription() },
        new() { Value = FinancialCenter.PELI, Text = FinancialCenter.PELI.GetDescription() }
    ];

    public static List<PaymentFrequencyOption> GetPaymentFrequencyOptions() =>
    [
        new() { Value = PaymentFrequency.Monthly,     Text = PaymentFrequency.Monthly.GetDescription() },
        new() { Value = PaymentFrequency.ThreeMonths, Text = PaymentFrequency.ThreeMonths.GetDescription() },
        new() { Value = PaymentFrequency.SixMonths,   Text = PaymentFrequency.SixMonths.GetDescription() },
        new() { Value = PaymentFrequency.Yearly,      Text = PaymentFrequency.Yearly.GetDescription() },
        new() { Value = PaymentFrequency.TwoYear,     Text = PaymentFrequency.TwoYear.GetDescription() },
        new() { Value = PaymentFrequency.ThreeYear,   Text = PaymentFrequency.ThreeYear.GetDescription() },
        new() { Value = PaymentFrequency.FourYear,    Text = PaymentFrequency.FourYear.GetDescription() },
        new() { Value = PaymentFrequency.FiveYear,    Text = PaymentFrequency.FiveYear.GetDescription() }
    ];

    public static List<DayCountConvOption> GetDayCountConvOptions() =>
    [
        new() { Value = DayCountConv.ACT_360,      Text = DayCountConv.ACT_360.GetDescription() },
        new() { Value = DayCountConv.ACT_365Fixed, Text = DayCountConv.ACT_365Fixed.GetDescription() },
        new() { Value = DayCountConv.ACT_ISDA,     Text = DayCountConv.ACT_ISDA.GetDescription() },
        new() { Value = DayCountConv._30_360,      Text = DayCountConv._30_360.GetDescription() },
        new() { Value = DayCountConv._30E_360,     Text = DayCountConv._30E_360.GetDescription() }
    ];

    public static List<PayBusDayConvOption> GetPayBusDayConvOptions() =>
    [
        new() { Value = PayBusDayConv.Following,    Text = PayBusDayConv.Following.GetDescription() },
        new() { Value = PayBusDayConv.ModFollowing, Text = PayBusDayConv.ModFollowing.GetDescription() },
        new() { Value = PayBusDayConv.Preceding,    Text = PayBusDayConv.Preceding.GetDescription() },
        new() { Value = PayBusDayConv.ModPreceding, Text = PayBusDayConv.ModPreceding.GetDescription() },
        new() { Value = PayBusDayConv.FRN,          Text = PayBusDayConv.FRN.GetDescription() }
    ];

    public static List<AmortizationMethodOption> GetAmortizationMethodOptions() =>
    [
        new() { Value = AmortizationMethod.None, Text = AmortizationMethod.None.GetDescription() },
        new() { Value = AmortizationMethod.TotalCashFlowMethod, Text = AmortizationMethod.TotalCashFlowMethod.GetDescription() },
        new() { Value = AmortizationMethod.Straightline, Text = AmortizationMethod.Straightline.GetDescription() },
        new() { Value = AmortizationMethod.IntrinsicValueMethod, Text = AmortizationMethod.IntrinsicValueMethod.GetDescription() },
        new() { Value = AmortizationMethod.Swaplet, Text = AmortizationMethod.Swaplet.GetDescription() }
    ];

    public static List<IntrinsicAmortizationMethodOption> GetIntrinsicAmortizationMethodOptions() =>
    [
        new() { Value = AmortizationMethod.None, Text = AmortizationMethod.None.GetDescription() },
        new() { Value = AmortizationMethod.TotalCashFlowMethod, Text = AmortizationMethod.TotalCashFlowMethod.GetDescription() },
        new() { Value = AmortizationMethod.Straightline, Text = AmortizationMethod.Straightline.GetDescription() },
        new() { Value = AmortizationMethod.IntrinsicValueMethod, Text = AmortizationMethod.IntrinsicValueMethod.GetDescription() },
        new() { Value = AmortizationMethod.Swaplet, Text = AmortizationMethod.Swaplet.GetDescription() }
    ];
    #endregion
}
