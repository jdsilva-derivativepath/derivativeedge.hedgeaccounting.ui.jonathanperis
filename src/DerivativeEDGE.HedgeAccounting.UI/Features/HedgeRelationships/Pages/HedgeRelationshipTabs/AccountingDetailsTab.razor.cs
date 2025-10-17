namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class AccountingDetailsTab
{
    [Parameter]
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }

    [Parameter]
    public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationshipChanged { get; set; }
}
