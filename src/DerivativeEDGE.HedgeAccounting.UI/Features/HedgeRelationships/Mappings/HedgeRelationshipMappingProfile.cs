namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Mappings;

public class HedgeRelationshipMappingProfile : Profile
{
    public HedgeRelationshipMappingProfile()
    {
        CreateMap<DerivativeEDGEHAEntityHedgeRelationship, DerivativeEDGEHAApiViewModelsHedgeRelationshipVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM, DerivativeEDGEHAEntityHedgeRelationship>();

        CreateMap<DerivativeEDGEHAEntityHedgeRegressionBatch, DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM, DerivativeEDGEHAEntityHedgeRegressionBatch>();

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipItem, DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM>()
            .ForMember(d => d.ItemID, o => o.NullSubstitute(string.Empty))
            .ForMember(d => d.Description, o => o.NullSubstitute(string.Empty));
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipItemVM, DerivativeEDGEHAEntityHedgeRelationshipItem>()
            .ForMember(d => d.ItemID, o => o.NullSubstitute(string.Empty))
            .ForMember(d => d.Description, o => o.NullSubstitute(string.Empty));

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipActivity, DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM, DerivativeEDGEHAEntityHedgeRelationshipActivity>();

        CreateMap<DerivativeEDGEHAEntityHedgeRelationshipOptionTimeValueAmort, DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM>();
        CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM, DerivativeEDGEHAEntityHedgeRelationshipOptionTimeValueAmort>();

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
}