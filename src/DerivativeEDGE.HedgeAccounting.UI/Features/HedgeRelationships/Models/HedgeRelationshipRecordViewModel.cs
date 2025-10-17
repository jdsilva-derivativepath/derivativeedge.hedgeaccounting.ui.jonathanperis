namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

public class HedgeRelationshipRecordViewModel
{
    private readonly DerivativeEDGEHAApiViewModelsHedgeRelationshipVM _source;

    public HedgeRelationshipRecordViewModel(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    // Computed properties
    public string? EntityName => _source.BankEntity?.LegalEntity?.Name;
    public string? HedgedItemID => _source.HedgedItem?.ItemID;

    // Proxy all original properties
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM Source => _source;
    public long ID => _source.ID;
    public long? ClientID => _source.ClientID;
    public string? Description => _source.Description;
    public string? HedgeTypeText => _source.HedgeTypeText;
    public double? Notional => _source.Notional;
    public string? DesignationDate => _source.DesignationDate;
    public string? DedesignationDate => _source.DedesignationDate;
    public string? HedgeRiskTypeText => _source.HedgeRiskTypeText;
    public string? HedgeStateText => _source.HedgeStateText;
    public string? HedgedItemTypeText => _source.HedgedItemTypeText;
    public bool CumulativeChanges => _source.CumulativeChanges;
    public bool PeriodicChanges => _source.PeriodicChanges;
    public int? Observation => _source.Observation;
    public string? PeriodSizeText => _source.PeriodSizeText;
    public string? BenchmarkText => _source.BenchmarkText;
    public string? HedgingInstrumentStructureText => _source.HedgingInstrumentStructureText;
    public bool IsAnOptionHedge => _source.IsAnOptionHedge;
    public bool OffMarket => _source.OffMarket;
    public bool QualitativeAssessment => _source.QualitativeAssessment;
    public bool PreIssuanceHedge => _source.PreIssuanceHedge;
    public bool IsDeltaMatchOption => _source.IsDeltaMatchOption;
    public string? FairValueMethodText => _source.FairValueMethodText;
    public bool Shortcut => _source.Shortcut;
    public bool? AvailableForSale => _source.AvailableForSale;
    public bool PortfolioLayerMethod => _source.PortfolioLayerMethod;

}