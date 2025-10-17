using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components
{
    public partial class HedgeRelationshipGrid
    {
        private DefaultGrid<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> _grid = null!;

        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public IEnumerable<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> Data { get; set; } = Array.Empty<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM>();
        [Parameter] public List<GridViewModel> GridViewItems { get; set; } = new();
        [Parameter] public EventCallback<GridViewModel> ViewCrudEvent { get; set; }
        [Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM?> OnRowClicked { get; set; }
        [Parameter] public EventCallback<long> OnDeleteRequested { get; set; }

        public void SetView(string filter) => _grid?.SetView(filter);

        private async Task OnViewCrudEventInternal(GridViewModel view)
        {
            if (ViewCrudEvent.HasDelegate)
                await ViewCrudEvent.InvokeAsync(view);
        }

        private void HandleRowClicked(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM? item)
        {
            if (OnRowClicked.HasDelegate)
                OnRowClicked.InvokeAsync(item);
        }
    }
}
