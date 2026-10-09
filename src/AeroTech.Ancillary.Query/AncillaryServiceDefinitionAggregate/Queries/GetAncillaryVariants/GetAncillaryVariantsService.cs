using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants
{
    public sealed class GetAncillaryVariantsService : IGetAncillaryVariantsService
    {
        public Task<IReadOnlyList<AncillaryVariantDto>> ExecuteAsync(IAncillaryVariantsQuery query, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AncillaryVariantDto> variants = AncillaryVariant.All
                .Where(variant => query.Profile is null || variant.Profile == query.Profile)
                .OrderBy(variant => variant.Code, StringComparer.Ordinal)
                .Select(variant => new AncillaryVariantDto(
                    variant.Code,
                    variant.Name,
                    EnumValueDto.Of(variant.Profile),
                    EnumValueDto.Of(variant.ApplicationType),
                    variant.PricingUnits.Select(EnumValueDto.Of).ToList(),
                    variant.ServiceDateBases.Select(EnumValueDto.Of).ToList()))
                .ToList();

            return Task.FromResult(variants);
        }
    }
}
