using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class PetSpecification
    {
        private const string Name = nameof(PetSpecification);
        private const int DocumentCodeMaxLength = 30;

        private readonly List<PetAnimal> _allowedAnimalTypes = new();
        private readonly List<SpecificationCode> _requiredDocumentCodes = new();
        private readonly List<PetSizeBracket> _allowedHoldAnimalSizeBrackets = new();

        private PetSpecification()
        {
        }

        public PetTransportMode TransportMode { get; private set; }

        public decimal MaxCombinedWeightKg { get; private set; }

        public DimensionsCm CarrierDimensionsMaxCm { get; private set; } = default!;

        public int? MinAnimalAgeWeeks { get; private set; }

        public IReadOnlyCollection<PetAnimal> AllowedAnimalTypes => _allowedAnimalTypes.AsReadOnly();

        public IReadOnlyCollection<SpecificationCode> RequiredDocumentCodes => _requiredDocumentCodes.AsReadOnly();

        public IReadOnlyCollection<PetSizeBracket> AllowedHoldAnimalSizeBrackets => _allowedHoldAnimalSizeBrackets.AsReadOnly();

        internal static PetSpecification Create(AncillaryVariant variant, PetSpecificationArgs args)
        {
            var isHold = variant.Code == AncillaryVariant.PetInHold;
            var animals = (args.AllowedAnimalTypes ?? []).Select(PetAnimal.Create).ToList();
            var documents = SpecificationRules.Codes(args.RequiredDocumentCodes, DocumentCodeMaxLength, Name, nameof(RequiredDocumentCodes));
            var brackets = (args.AllowedHoldAnimalSizeBrackets ?? []).Select(PetSizeBracket.Create).OrderBy(bracket => bracket.WeightFromExclusiveKg).ToList();

            Require(args.TransportMode == (isHold ? PetTransportMode.Hold : PetTransportMode.Cabin), nameof(TransportMode));
            Require(animals.Count > 0, nameof(AllowedAnimalTypes));
            Require(animals.Select(animal => (animal.AnimalType, animal.OtherCode)).Distinct().Count() == animals.Count, nameof(AllowedAnimalTypes));
            Require(args.MaxCombinedWeightKg > 0 && SpecificationRules.IsMeasure(args.MaxCombinedWeightKg), nameof(MaxCombinedWeightKg));
            Require(args.CarrierDimensionsMaxCm is not null, nameof(CarrierDimensionsMaxCm));
            Require(args.MinAnimalAgeWeeks is null or >= 0, nameof(MinAnimalAgeWeeks));
            Require(isHold || brackets.Count == 0, nameof(AllowedHoldAnimalSizeBrackets));
            Require(brackets.Select(bracket => bracket.Code).Distinct(StringComparer.Ordinal).Count() == brackets.Count, nameof(AllowedHoldAnimalSizeBrackets));
            Require(
                brackets.Zip(brackets.Skip(1), (lower, upper) => lower.WeightToInclusiveKg <= upper.WeightFromExclusiveKg).All(disjoint => disjoint),
                nameof(AllowedHoldAnimalSizeBrackets));
            Require(brackets.All(bracket => bracket.WeightToInclusiveKg <= args.MaxCombinedWeightKg), nameof(AllowedHoldAnimalSizeBrackets));

            var specification = new PetSpecification
            {
                TransportMode = args.TransportMode,
                MaxCombinedWeightKg = args.MaxCombinedWeightKg,
                CarrierDimensionsMaxCm = DimensionsCm.Create(args.CarrierDimensionsMaxCm!, Name, nameof(CarrierDimensionsMaxCm)),
                MinAnimalAgeWeeks = args.MinAnimalAgeWeeks
            };

            specification._allowedAnimalTypes.AddRange(animals);
            specification._requiredDocumentCodes.AddRange(documents);
            specification._allowedHoldAnimalSizeBrackets.AddRange(brackets);

            return specification;
        }

        public PetSpecificationArgs ToArgs()
            => new(
                TransportMode,
                _allowedAnimalTypes.Select(animal => new PetAnimalArgs(animal.AnimalType, animal.OtherCode)).ToList(),
                MaxCombinedWeightKg,
                CarrierDimensionsMaxCm.ToArgs(),
                MinAnimalAgeWeeks,
                _requiredDocumentCodes.Select(row => row.Code).ToList(),
                _allowedHoldAnimalSizeBrackets.Select(bracket => new PetSizeBracketArgs(bracket.Code, bracket.WeightFromExclusiveKg, bracket.WeightToInclusiveKg)).ToList());

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
