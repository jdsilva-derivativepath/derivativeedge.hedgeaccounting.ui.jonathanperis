using EnumDescriptionAttribute = DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Attributes.EnumDescriptionAttribute;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Enums;

[Flags]
public enum HedgingInstrumentStructure : int
{
    [EnumDescription("Single Instrument")]
    SingleInstrument = 1,

    [EnumDescription("Structured Product")]
    StructuredProduct = 2,

    [EnumDescription("Multiple Instruments")]
    MultipleInstruments = 3
}