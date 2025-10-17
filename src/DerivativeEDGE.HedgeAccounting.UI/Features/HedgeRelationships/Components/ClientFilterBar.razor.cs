using Syncfusion.Blazor.Calendars;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Components;

public partial class ClientFilterBar
{
    [Parameter] public List<Client> Clients { get; set; } = new();
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public long? SelectedClientId { get; set; }
    [Parameter] public DateTime? CurveDate { get; set; }
    [Parameter] public EventCallback<long?> SelectedClientIdChanged { get; set; }
    [Parameter] public EventCallback<DateTime?> CurveDateChanged { get; set; }
    [Parameter] public EventCallback OnClientCreated { get; set; }

    private async Task ClientComboboxValueChange() =>
        await SelectedClientIdChanged.InvokeAsync(SelectedClientId);

    private async Task OnCurveDateChanged(ChangedEventArgs<DateTime?> args) =>
        await CurveDateChanged.InvokeAsync(args.Value);

    private async Task OnCreated() => await OnClientCreated.InvokeAsync();
}
