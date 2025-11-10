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
    [Parameter] public List<ToolbarItemModel> BasicTools { get; set; }
    
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
    [Parameter] public EventCallback<Syncfusion.Blazor.Buttons.ChangeEventArgs<bool>> OnIsAnOptionHedgeChanged { get; set; }
    [Parameter] public EventCallback<Syncfusion.Blazor.Buttons.ChangeEventArgs<bool>> OnOffMarketChanged { get; set; }
    [Parameter] public EventCallback<Syncfusion.Blazor.Buttons.ChangeEventArgs<bool>> OnExcludeIntrinsicValueChanged { get; set; }
    
    [Parameter] public EventCallback OnSelectTemplateClick { get; set; }
    [Parameter] public EventCallback OnPreviewDocumentLegacyClick { get; set; }
    [Parameter] public EventCallback OnPreviewDocumentNewClick { get; set; }
    [Parameter] public EventCallback OnEditDocumentClick { get; set; }
    #endregion

    private string GetTruncatedObjective()
    {
        if (string.IsNullOrEmpty(HedgeRelationship?.Objective))
            return string.Empty;

        if (HedgeRelationship.Objective.Length <= 200)
            return HedgeRelationship.Objective;

        return HedgeRelationship.Objective[..200];
    }

    #region Event Handlers
    /// <summary>
    /// Handles HedgeType dropdown value changes.
    /// Legacy reference: old/hr_hedgeRelationshipAddEditCtrl.js line 234 - $watch on HedgeType triggers setDropDownListEffectivenessMethods()
    /// </summary>
    private async Task OnHedgeTypeChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<DerivativeEDGEHAEntityEnumHRHedgeType, DropdownModel> args)
    {
        if (HedgeRelationship != null)
        {
            HedgeRelationship.HedgeType = args.Value;
            
            // Notify parent component that HedgeRelationship has changed
            // This will trigger refresh of effectiveness method dropdown options in InstrumentAnalysisTab
            await HedgeRelationshipChanged.InvokeAsync(HedgeRelationship);
        }
    }
    #endregion
}
