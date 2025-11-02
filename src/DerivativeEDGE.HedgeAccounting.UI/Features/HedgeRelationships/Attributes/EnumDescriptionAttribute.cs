namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Attributes;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
public sealed class EnumDescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
