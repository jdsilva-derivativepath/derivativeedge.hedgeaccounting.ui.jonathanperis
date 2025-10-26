namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Mappings;

public class HedgeRelationshipMappingProfile : Profile
{
    public HedgeRelationshipMappingProfile()
    {
        CreateMap<DerivativeEDGEHAEntityHedgeRelationship, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM, DerivativeEDGEHAEntityHedgeRelationship>()
            .ForMember(dest => dest.DesignationDate, opt => opt.MapFrom(src => ParseDateTimeOffset(src.DesignationDate)))
            .ForMember(dest => dest.DedesignationDate, opt => opt.MapFrom(src => ParseNullableDateTimeOffset(src.DedesignationDate)))
            .ForMember(dest => dest.HedgedItems, opt => opt.MapFrom(src => src.HedgedItems))
            .ForMember(dest => dest.HedgingItems, opt => opt.MapFrom(src => src.HedgingItems))
            .ForMember(dest => dest.HedgeRegressionBatches, opt => opt.MapFrom(src => src.HedgeRegressionBatches))
            .ForMember(dest => dest.HedgeRelationshipActivities, opt => opt.MapFrom(src => src.HedgeRelationshipActivities))
            .ForMember(dest => dest.HedgeRelationshipOptionTimeValues, opt => opt.MapFrom(src => src.HedgeRelationshipOptionTimeValues));

        CreateMap<DerivativeEDGEHAEntityHedgeRegressionBatch, DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM, DerivativeEDGEHAEntityHedgeRegressionBatch>()
            .ForMember(dest => dest.HedgeRegressionBatchResults, opt => opt.MapFrom(src => src.HedgeRegressionBatchResults))
            .ForMember(dest => dest.HedgeRelationshipLogs, opt => opt.MapFrom(src => src.HedgeRelationshipLogs))
            .ForMember(dest => dest.ValueDate, opt => opt.MapFrom(src => ParseDateTimeOffset(src.ValueDate)));

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipItem, DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>()
            .ForMember(d => d.ItemID, o => o.NullSubstitute(string.Empty))
            .ForMember(d => d.Description, o => o.NullSubstitute(string.Empty))
            .ForMember(dest => dest.HedgeRelationshipItemLegs, opt => opt.MapFrom(src => src.HedgeRelationshipItemLegs));
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM, DerivativeEDGEHAEntityHedgeRelationshipItem>()
            .ForMember(d => d.ItemID, o => o.NullSubstitute(string.Empty))
            .ForMember(d => d.Description, o => o.NullSubstitute(string.Empty))
            .ForMember(dest => dest.HedgeRelationshipItemLegs, opt => opt.MapFrom(src => src.HedgeRelationshipItemLegs));

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipActivity, DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM, DerivativeEDGEHAEntityHedgeRelationshipActivity>();

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipOptionTimeValueAmort, DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM, DerivativeEDGEHAEntityHedgeRelationshipOptionTimeValueAmort>()
            .ForMember(dest => dest.OptionTimeValueAmortRollSchedules, opt => opt.MapFrom(src => src.OptionTimeValueAmortRollSchedules))
            .ForMember(dest => dest.OptionAmortizations, opt => opt.MapFrom(src => src.OptionAmortizations))
            .ForMember(dest => dest.OptionSwapletAmortizations, opt => opt.MapFrom(src => src.OptionSwapletAmortizations))
            .ForMember(dest => dest.FinancialCenters, opt => opt.MapFrom(src => src.FinancialCenters));

        // Mappings for nested collection element types
        CreateMap<DerivativeEDGEHAEntityOptionAmortization, DerivativeEDGEHAApiViewModelsOptionAmortizationVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsOptionAmortizationVM, DerivativeEDGEHAEntityOptionAmortization>();

        CreateMap<DerivativeEDGEHAEntityOptionTimeValueAmortRollSchedule, DerivativeEDGEHAApiViewModelsOptionTimeValueAmortRollScheduleVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsOptionTimeValueAmortRollScheduleVM, DerivativeEDGEHAEntityOptionTimeValueAmortRollSchedule>();

        CreateMap<DerivativeEDGEHAEntityOptionSwapletAmortization, DerivativeEDGEHAApiViewModelsOptionSwapletAmortizationVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsOptionSwapletAmortizationVM, DerivativeEDGEHAEntityOptionSwapletAmortization>();

        CreateMap<DerivativeEDGEHAEntityClient, DerivativeEDGEHAApiViewModelsClientVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsClientVM, DerivativeEDGEHAEntityClient>();

        CreateMap<DerivativeEDGEHAEntityBankEntity, DerivativeEDGEHAApiViewModelsBankEntityVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsBankEntityVM, DerivativeEDGEHAEntityBankEntity>();

        CreateMap<DerivativeEDGEHAEntityEffectivenessMethod, DerivativeEDGEHAApiViewModelsEffectivenessMethodVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsEffectivenessMethodVM, DerivativeEDGEHAEntityEffectivenessMethod>();

        CreateMap<DerivativeEDGEHAEntityLegalEntity, DerivativeEDGEHAApiViewModelsLegalEntityVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsLegalEntityVM, DerivativeEDGEHAEntityLegalEntity>();

        CreateMap<DerivativeEDGEHAEntityUser, DerivativeEDGEHAApiViewModelsUserVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsUserVM, DerivativeEDGEHAEntityUser>();

        CreateMap<DerivativeEDGEHAEntityClientConfig, DerivativeEDGEHAApiViewModelsClientConfigVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsClientConfigVM, DerivativeEDGEHAEntityClientConfig>();

        CreateMap<DerivativeEDGEHAEntityGLAccount, DerivativeEDGEHAApiViewModelsGLAccountVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsGLAccountVM, DerivativeEDGEHAEntityGLAccount>();

        CreateMap<DerivativeEDGEHAEntityHedgeRegressionBatchResult, DerivativeEDGEHAApiViewModelsHedgeRegressionBatchResultVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchResultVM, DerivativeEDGEHAEntityHedgeRegressionBatchResult>();

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipLog, DerivativeEDGEHAApiViewModelsHedgeRelationshipLogVm>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipLogVm, DerivativeEDGEHAEntityHedgeRelationshipLog>();

        CreateMap<DerivativeEDGEHAEntityPerson, DerivativeEDGEHAApiViewModelsPersonVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsPersonVM, DerivativeEDGEHAEntityPerson>();

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipItemLeg, DerivativeEDGEHAApiViewModelsHedgeRelationshipItemLegVm>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemLegVm, DerivativeEDGEHAEntityHedgeRelationshipItemLeg>();
    }

    /// <summary>
    /// Parses a string date to DateTimeOffset. Returns DateTimeOffset.MinValue if parsing fails or string is null/empty.
    /// </summary>
    private static DateTimeOffset ParseDateTimeOffset(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return DateTimeOffset.MinValue;
        }

        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            return new DateTimeOffset(parsedDate);
        }

        return DateTimeOffset.MinValue;
    }

    /// <summary>
    /// Parses a string date to nullable DateTimeOffset. Returns null if parsing fails or string is null/empty.
    /// </summary>
    private static DateTimeOffset? ParseNullableDateTimeOffset(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            return new DateTimeOffset(parsedDate);
        }

        return null;
    }
}