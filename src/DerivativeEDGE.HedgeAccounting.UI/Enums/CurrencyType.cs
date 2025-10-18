namespace DerivativeEDGE.HedgeAccounting.UI.Enums;

public abstract class Enumeration : IComparable
{
    public string Text { get; private set; }
    public string Id { get; private set; }
    public string Fimg { get; private set; }

    protected Enumeration(string id, string text, string fimg) => (Id, Text, Fimg) = (id, text, fimg);

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
                 .Select(f => f.GetValue(null))
                 .Cast<T>();

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = !string.IsNullOrWhiteSpace(Id) && Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }
    public override int GetHashCode() => Id!.GetHashCode();

    public int CompareTo(object? obj) => string.Compare(Id, ((Enumeration)obj!).Id, false, CultureInfo.InvariantCulture);

    public static bool operator <(Enumeration left, Enumeration right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Enumeration left, Enumeration right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Enumeration left, Enumeration right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Enumeration left, Enumeration right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
public class CurrencyType(string id, string text, string fimg) : Enumeration(id, text, fimg)
{
    public static readonly CurrencyType AUD = new("AUD", nameof(AUD), "flag-australia");
    public static readonly CurrencyType BRL = new("BRL", nameof(BRL), "flag-brazil");
    public static readonly CurrencyType CAD = new("CAD", nameof(CAD), "flag-canada");
    public static readonly CurrencyType USD = new("USD", nameof(USD), "flag-us");
}
