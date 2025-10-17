namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class HedgeRelationshipGrid
{
    private DefaultGrid<HedgeRelationshipRecordViewModel> _grid = null!;

    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public IEnumerable<HedgeRelationshipRecordViewModel> Data { get; set; } = Array.Empty<HedgeRelationshipRecordViewModel>();
    [Parameter] public List<GridViewModel> GridViewItems { get; set; } = new();
    [Parameter] public EventCallback<GridViewModel> ViewCrudEvent { get; set; }
    [Parameter] public EventCallback<HedgeRelationshipRecordViewModel?> OnRowClicked { get; set; }
    [Parameter] public EventCallback<long> OnDeleteRequested { get; set; }

    public void SetView(string filter) => _grid?.SetView(filter);

    private async Task OnViewCrudEventInternal(GridViewModel view)
    {
        if (ViewCrudEvent.HasDelegate)
            await ViewCrudEvent.InvokeAsync(view);
    }

    private void HandleRowClicked(HedgeRelationshipRecordViewModel? item)
    {
        if (OnRowClicked.HasDelegate)
            OnRowClicked.InvokeAsync(item);
    }
}
