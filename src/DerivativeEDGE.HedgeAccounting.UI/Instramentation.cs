using System.Diagnostics.Metrics;

namespace DerivativeEDGE.HedgeAccounting.UI;

public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "DerivativeEDGE.HedgeAccounting.UI";
    internal const string MeterName = "DerivativeEDGE.HedgeAccounting.UI.Meter";
    private readonly Meter meter;

    public Instrumentation()
    {
        string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(ActivitySourceName, version);
        meter = new Meter(MeterName, version);
        CustomCounter = meter.CreateCounter<long>("HedgeAccounting.UI.Counter", "Custom Counter");
    }

    public ActivitySource ActivitySource { get; }

    public Counter<long> CustomCounter { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        meter.Dispose();
    }
}