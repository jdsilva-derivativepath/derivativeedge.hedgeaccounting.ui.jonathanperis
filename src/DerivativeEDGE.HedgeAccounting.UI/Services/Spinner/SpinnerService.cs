namespace DerivativeEDGE.HedgeAccounting.UI.Services.Spinner;

public class SpinnerService
{
    public event Action OnShow;
    public event Action OnHide;

    public void Hide()
    {
        OnHide?.Invoke();
    }

    public void Show()
    {
        OnShow?.Invoke();
    }
}
