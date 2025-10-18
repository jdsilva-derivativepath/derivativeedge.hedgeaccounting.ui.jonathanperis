using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;
using DerivativeEDGE.HedgeAccounting.UI.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class HedgeRelationshipInfoSection
{
    #region Parameters
    [Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
    [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationshipChanged { get; set; }
    
    [Parameter] public bool IsLoadingClients { get; set; }
    [Parameter] public bool IsLoadingEntities { get; set; }
    [Parameter] public List<Client> AvailableClients { get; set; }
    [Parameter] public List<DerivativeEDGEHAEntityLegalEntity> AvailableEntities { get; set; }
    
    [Parameter] public string TemplateDisplayName { get; set; }
    [Parameter] public bool IsNewHedgeDocumentTemplate { get; set; }
    [Parameter] public List<object> BasicTools { get; set; }
    
    [Parameter] public string BenchMarkLabel { get; set; }
    [Parameter] public bool CanEditFairValueMethod { get; set; }
    [Parameter] public bool IsHedgingInstrumentStructureDisabled { get; set; }
    [Parameter] public List<HedgingInstrumentStructureOption> HedgingInstrumentStructureOptions { get; set; }
    
    [Parameter] public DateTime? DesignationDate { get; set; }
    [Parameter] public EventCallback<DateTime?> DesignationDateChanged { get; set; }
    [Parameter] public DateTime? DeDesignationDate { get; set; }
    [Parameter] public EventCallback<DateTime?> DeDesignationDateChanged { get; set; }
    
    [Parameter] public bool CanEditCheckbox { get; set; }
    [Parameter] public bool CanEditPreIssuanceHedge { get; set; }
    [Parameter] public bool CanEditPortfolioLayerMethod { get; set; }
    
    [Parameter] public List<ChartDataModel> EffectivenessChartData { get; set; }
    
    // Event callbacks
    [Parameter] public EventCallback<ChangeEventArgs<long, Client>> OnClientValueChange { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs<bool>> OnIsAnOptionHedgeChanged { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs<bool>> OnOffMarketChanged { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs<bool>> OnExcludeIntrinsicValueChanged { get; set; }
    
    [Parameter] public EventCallback OnSelectTemplateClick { get; set; }
    [Parameter] public EventCallback OnPreviewDocumentLegacyClick { get; set; }
    [Parameter] public EventCallback OnPreviewDocumentNewClick { get; set; }
    [Parameter] public EventCallback OnEditDocumentClick { get; set; }
    #endregion
}
