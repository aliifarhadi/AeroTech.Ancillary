using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class BassinetDetails
    {
        private const string Name = nameof(BassinetDetails);
        private const int SeatGroupMaxLength = 25;

        private readonly List<SpecificationCode> _compatibleSeatGroups = new();

        private BassinetDetails()
        {
        }

        public decimal? MaxInfantWeightKg { get; private set; }

        public int? MaxInfantAgeMonths { get; private set; }

        public bool RequiresInfantAndGuardian { get; private set; }

        public IReadOnlyCollection<SpecificationCode> CompatibleSeatGroups => _compatibleSeatGroups.AsReadOnly();

        internal static BassinetDetails Create(BassinetDetailsArgs args)
        {
            var seatGroups = SpecificationRules.Codes(args.CompatibleSeatGroups, SeatGroupMaxLength, Name, nameof(CompatibleSeatGroups));

            SpecificationRules.Require(SpecificationRules.IsMeasure(args.MaxInfantWeightKg), Name, nameof(MaxInfantWeightKg));
            SpecificationRules.Require(args.MaxInfantAgeMonths is null or > 0, Name, nameof(MaxInfantAgeMonths));
            SpecificationRules.Require(args.MaxInfantWeightKg is not null || args.MaxInfantAgeMonths is not null, Name, nameof(MaxInfantWeightKg));
            SpecificationRules.Require(seatGroups.Count > 0, Name, nameof(CompatibleSeatGroups));
            SpecificationRules.Require(args.RequiresInfantAndGuardian, Name, nameof(RequiresInfantAndGuardian));

            var details = new BassinetDetails
            {
                MaxInfantWeightKg = args.MaxInfantWeightKg,
                MaxInfantAgeMonths = args.MaxInfantAgeMonths,
                RequiresInfantAndGuardian = args.RequiresInfantAndGuardian
            };

            details._compatibleSeatGroups.AddRange(seatGroups);

            return details;
        }

        public BassinetDetailsArgs ToArgs()
            => new(MaxInfantWeightKg, MaxInfantAgeMonths, _compatibleSeatGroups.Select(row => row.Code).ToList(), RequiresInfantAndGuardian);
    }
}
