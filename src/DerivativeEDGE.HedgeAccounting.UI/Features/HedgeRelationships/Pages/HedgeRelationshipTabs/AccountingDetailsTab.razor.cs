using DerivativeEDGE.HedgeAccounting.Api.Client;
using Microsoft.AspNetCore.Components;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class AccountingDetailsTab
{
    [Parameter]
    public DerivativeEDGEHAEntityHedgeRelationship? HedgeRelationship { get; set; }

    [Parameter]
    public EventCallback<DerivativeEDGEHAEntityHedgeRelationship?> HedgeRelationshipChanged { get; set; }
}
