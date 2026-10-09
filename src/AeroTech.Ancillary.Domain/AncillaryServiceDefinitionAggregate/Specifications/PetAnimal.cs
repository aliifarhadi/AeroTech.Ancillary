using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class PetAnimal
    {
        private const int OtherCodeMaxLength = 10;

        private PetAnimal()
        {
        }

        public PetAnimalType AnimalType { get; private set; }

        public string OtherCode { get; private set; } = default!;

        internal static PetAnimal Create(PetAnimalArgs args)
        {
            var isOther = args is { AnimalType: PetAnimalType.RegisteredOther };

            SpecificationRules.Require(args is not null && Enum.IsDefined(args.AnimalType), nameof(PetSpecification), nameof(AnimalType));
            SpecificationRules.Require(
                isOther ? SpecificationRules.IsCode(args!.OtherCode, OtherCodeMaxLength) : string.IsNullOrEmpty(args!.OtherCode),
                nameof(PetSpecification),
                nameof(OtherCode));

            return new PetAnimal { AnimalType = args.AnimalType, OtherCode = args.OtherCode ?? string.Empty };
        }
    }
}
