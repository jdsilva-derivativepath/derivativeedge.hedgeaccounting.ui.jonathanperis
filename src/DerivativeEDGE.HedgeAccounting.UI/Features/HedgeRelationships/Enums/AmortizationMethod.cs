using EnumDescriptionAttribute = DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Attributes.EnumDescriptionAttribute;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Enums;

[Flags]
public enum AmortizationMethod : int
{
    [EnumDescription("None")]
    None = 0,

    [EnumDescription("Total Cash Flow Method")]
    TotalCashFlowMethod = 1,

    [EnumDescription("Straightline")]
    Straightline = 2,

    [EnumDescription("Intrinsic Value Method")]
    IntrinsicValueMethod = 3,

    [EnumDescription("Swaplet")]
    Swaplet = 4,
}
