using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm
{
    public static class ProfileProvisionFormMapper
    {
        public static ProvisionConstraintsDto Constraints(AncillaryServiceDefinition definition)
            => new(
                EnumValueDto.Of(AncillaryProvision.QuantityUnitOf(definition) ?? throw ExceptionFactory.ServiceDefinitionNotClassified()),
                AncillaryProvision.MaxQuantityOf(definition),
                AncillaryProvision.AllowedDispositions(definition).Select(disposition => EnumValueDto.Of(disposition)).ToList(),
                AncillaryProvision.AllowedPriceOrigins(definition).Select(priceOrigin => EnumValueDto.Of(priceOrigin)).ToList(),
                AncillaryProvision.AllowedPurchaseStages(definition).Select(purchaseStage => EnumValueDto.Of(purchaseStage)).ToList());

        public static string? ForeignRuleSection(ProvisionFormDto form, string? profileRuleSection)
            => new (string Section, bool Present)[]
                {
                    (nameof(ProvisionFormDto.BaggageApplication), form.BaggageApplication is not null),
                    (nameof(ProvisionFormDto.SeatApplication), form.SeatApplication is not null),
                    (nameof(ProvisionFormDto.PetRule), form.PetRule is not null),
                    (nameof(ProvisionFormDto.AssistedTravelRule), form.AssistedTravelRule is not null),
                    (nameof(ProvisionFormDto.AirportServiceRule), form.AirportServiceRule is not null)
                }
                .FirstOrDefault(rule => rule.Present && rule.Section != profileRuleSection)
                .Section;

        public static BackofficeProfileProvisionFormDto<TForm> Reshape<TForm>(BackofficeProfileProvisionFormDto<ProvisionFormDto> provision, Func<ProvisionFormDto, TForm> shape)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Status,
                provision.Profile,
                provision.VariantCode,
                provision.Editable,
                provision.AllowedRuleSections,
                provision.Constraints,
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                shape(provision.Form));
    }
}
