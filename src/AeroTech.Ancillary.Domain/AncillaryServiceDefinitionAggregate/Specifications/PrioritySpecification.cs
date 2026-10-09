using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class PrioritySpecification
    {
        private const string Name = nameof(PrioritySpecification);
        private const int CodeMaxLength = 20;
        private const int FareBenefitRefMaxLength = 40;

        private readonly List<SpecificationReference> _airports = new();

        private PrioritySpecification()
        {
        }

        public PriorityKind Kind { get; private set; }

        public string? PriorityZoneCode { get; private set; }

        public string? PriorityGroupCode { get; private set; }

        public string? FareBenefitRef { get; private set; }

        public IReadOnlyCollection<SpecificationReference> Airports => _airports.AsReadOnly();

        internal static PrioritySpecification Create(PrioritySpecificationArgs args)
        {
            var airports = SpecificationRules.References(args.AirportIds?.Select(id => (long)id), Name, nameof(Airports));

            Require(Enum.IsDefined(args.Kind), nameof(Kind));
            Require(args.PriorityZoneCode is null || SpecificationRules.IsCode(args.PriorityZoneCode, CodeMaxLength), nameof(PriorityZoneCode));
            Require(args.PriorityGroupCode is null || SpecificationRules.IsCode(args.PriorityGroupCode, CodeMaxLength), nameof(PriorityGroupCode));
            Require(args.FareBenefitRef is null || SpecificationRules.IsCode(args.FareBenefitRef, FareBenefitRefMaxLength), nameof(FareBenefitRef));

            var specification = new PrioritySpecification
            {
                Kind = args.Kind,
                PriorityZoneCode = args.PriorityZoneCode,
                PriorityGroupCode = args.PriorityGroupCode,
                FareBenefitRef = args.FareBenefitRef
            };

            specification._airports.AddRange(airports);

            return specification;
        }

        public PrioritySpecificationArgs ToArgs()
            => new(Kind, PriorityZoneCode, PriorityGroupCode, FareBenefitRef, _airports.Select(row => (int)row.ReferenceId).ToList());

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
