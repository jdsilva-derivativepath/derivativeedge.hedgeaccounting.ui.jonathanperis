namespace DerivativeEDGE.HedgeAccounting.UI.Components;

public partial class Spinner
{
    private bool _visible = false;

    [Inject]
    private SpinnerService SpinnerService { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        SpinnerService.OnShow += ShowSpinner;
        SpinnerService.OnHide += HideSpinner;
        return base.OnInitializedAsync();
    }

    private void ShowSpinner()
    {
        var originalValue = _visible;
        _visible = true;
        if (_visible != originalValue)
        {
            SpinnerService.Show();
            StateHasChanged();
        }
    }

    private void HideSpinner()
    {
        var originalValue = _visible;
        _visible = false;
        if (_visible != originalValue)
        {
            SpinnerService.Hide();
            StateHasChanged();
        }
    }
}
