namespace DerivativeEDGE.HedgeAccounting.UI.Attributes;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
public sealed class EnumDescriptionAttribute : Attribute
{
    public string Description { get; }

    public EnumDescriptionAttribute(string description)
    {
        Description = description;
    }
}
