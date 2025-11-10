using SystemAction = System.Action;

namespace DerivativeEDGE.HedgeAccounting.UI.Services.Spinner;

public class SpinnerService
{
    public event SystemAction OnShow;
    public event SystemAction OnHide;

    public void Hide()
    {
        OnHide?.Invoke();
    }

    public void Show()
    {
        OnShow?.Invoke();
    }
}
