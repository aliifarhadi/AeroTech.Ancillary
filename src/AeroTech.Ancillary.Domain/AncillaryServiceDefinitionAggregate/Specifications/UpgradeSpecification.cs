using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class UpgradeSpecification
    {
        private const string Name = nameof(UpgradeSpecification);

        private readonly List<SpecificationReference> _eligibleFareFamilies = new();

        private UpgradeSpecification()
        {
        }

        public int FromCabinId { get; private set; }

        public int ToCabinId { get; private set; }

        public UpgradeKind AllowedUpgradeKind { get; private set; }

        public bool RequiresTicketExchange { get; private set; }

        public IReadOnlyCollection<SpecificationReference> EligibleFareFamilies => _eligibleFareFamilies.AsReadOnly();

        internal static UpgradeSpecification Create(UpgradeSpecificationArgs args)
        {
            var fareFamilies = SpecificationRules.References(args.EligibleFareFamilyIds, Name, nameof(EligibleFareFamilies));

            Require(args.FromCabinId > 0, nameof(FromCabinId));
            Require(args.ToCabinId > 0 && args.ToCabinId != args.FromCabinId, nameof(ToCabinId));
            Require(Enum.IsDefined(args.AllowedUpgradeKind), nameof(AllowedUpgradeKind));
            Require(args.AllowedUpgradeKind != UpgradeKind.TicketReprice || args.RequiresTicketExchange, nameof(RequiresTicketExchange));

            var specification = new UpgradeSpecification
            {
                FromCabinId = args.FromCabinId,
                ToCabinId = args.ToCabinId,
                AllowedUpgradeKind = args.AllowedUpgradeKind,
                RequiresTicketExchange = args.RequiresTicketExchange
            };

            specification._eligibleFareFamilies.AddRange(fareFamilies);

            return specification;
        }

        public UpgradeSpecificationArgs ToArgs()
            => new(FromCabinId, ToCabinId, AllowedUpgradeKind, RequiresTicketExchange, _eligibleFareFamilies.Select(row => row.ReferenceId).ToList());

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
