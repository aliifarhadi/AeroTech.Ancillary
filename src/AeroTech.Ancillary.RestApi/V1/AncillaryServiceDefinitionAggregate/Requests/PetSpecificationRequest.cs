using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1._Shared.OpenApi;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    [VariantSpecification(
        typeof(PetVariantSpecificationDto),
        "The pet fields of the variant named by variantCode: A13 Pet in cabin, A14 Pet in hold. The first v1 body is still accepted: " +
        "transportMode and allowedHoldAnimalSizeBrackets may be sent on every variant and must then agree with it, otherwise the answer is " +
        "422 with code 16202.")]
    public sealed record PetSpecificationRequest(
        PetTransportMode? TransportMode,
        IReadOnlyList<PetAnimalInput>? AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        DimensionsCmInput CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string>? RequiredDocumentCodes,
        IReadOnlyList<PetSizeBracketInput>? AllowedHoldAnimalSizeBrackets)
    {
        public PetSpecificationInput ToInput(string variantCode)
            => new(
                TransportMode ?? variantCode switch
                {
                    AncillaryVariant.PetInHold => PetTransportMode.Hold,
                    _ => PetTransportMode.Cabin
                },
                AllowedAnimalTypes,
                MaxCombinedWeightKg,
                CarrierDimensionsMaxCm,
                MinAnimalAgeWeeks,
                RequiredDocumentCodes,
                AllowedHoldAnimalSizeBrackets);
    }
}
