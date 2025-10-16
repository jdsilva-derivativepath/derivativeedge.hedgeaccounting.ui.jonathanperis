using EnumDescriptionAttribute = DerivativeEDGE.HedgeAccounting.UI.Attributes.EnumDescriptionAttribute;

namespace DerivativeEDGE.HedgeAccounting.UI.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        if (value == null) { return string.Empty; }

        FieldInfo field = value.GetType().GetField(value.ToString());
        if (field == null) { return value.ToString(); }

        EnumDescriptionAttribute attribute = field.GetCustomAttribute<EnumDescriptionAttribute>();

        if (attribute is null)
        {
            Common.Attributes.EnumDescriptionAttribute attributeEDGE = field.GetCustomAttribute<Common.Attributes.EnumDescriptionAttribute>();
            return attributeEDGE?.Description ?? value.ToString();
        }

        return attribute?.Description ?? value.ToString();
    }
}
