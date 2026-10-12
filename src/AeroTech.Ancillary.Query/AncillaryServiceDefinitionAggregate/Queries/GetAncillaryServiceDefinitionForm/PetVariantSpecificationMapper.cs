using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class PetVariantSpecificationMapper
    {
        public static PetVariantSpecificationDto ToDto(AncillaryServiceDefinition definition)
        {
            var pet = definition.Pet!.ToArgs();
            var animals = pet.AllowedAnimalTypes.Select(animal => new PetAnimalFormDto(animal.AnimalType, animal.OtherCode)).ToList();
            var carrier = new DimensionsCmDto(pet.CarrierDimensionsMaxCm.LengthCm, pet.CarrierDimensionsMaxCm.WidthCm, pet.CarrierDimensionsMaxCm.HeightCm);

            return definition.VariantCode switch
            {
                AncillaryVariant.PetInCabin => new PetInCabinSpecificationDto(
                    animals,
                    pet.MaxCombinedWeightKg,
                    carrier,
                    pet.MinAnimalAgeWeeks,
                    pet.RequiredDocumentCodes),
                _ => new PetInHoldSpecificationDto(
                    animals,
                    pet.MaxCombinedWeightKg,
                    carrier,
                    pet.MinAnimalAgeWeeks,
                    pet.RequiredDocumentCodes,
                    pet.AllowedHoldAnimalSizeBrackets
                        .Select(bracket => new PetSizeBracketDto(bracket.Code, bracket.WeightFromExclusiveKg, bracket.WeightToInclusiveKg))
                        .ToList())
            };
        }
    }
}
