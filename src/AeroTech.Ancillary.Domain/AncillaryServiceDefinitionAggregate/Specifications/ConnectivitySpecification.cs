using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class ConnectivitySpecification
    {
        private const string Name = nameof(ConnectivitySpecification);
        private const int ProviderRefMaxLength = 50;

        private readonly List<SpecificationReference> _eligibleAircraft = new();

        private ConnectivitySpecification()
        {
        }

        public ConnectivityPlanKind PlanKind { get; private set; }

        public int? DurationMinutes { get; private set; }

        public int? IncludedDataMb { get; private set; }

        public int? MaxDevices { get; private set; }

        public PurchaseStage DeliveryStage { get; private set; }

        public string? FulfillmentProviderRef { get; private set; }

        public IReadOnlyCollection<SpecificationReference> EligibleAircraft => _eligibleAircraft.AsReadOnly();

        internal static ConnectivitySpecification Create(ConnectivitySpecificationArgs args)
        {
            var aircraft = SpecificationRules.References(args.EligibleAircraftIds?.Select(id => (long)id), Name, nameof(EligibleAircraft));

            Require(Enum.IsDefined(args.PlanKind), nameof(PlanKind));
            Require(args.DurationMinutes is null or > 0, nameof(DurationMinutes));
            Require(args.IncludedDataMb is null or > 0, nameof(IncludedDataMb));
            Require(args.MaxDevices is null or > 0, nameof(MaxDevices));
            Require(args.PlanKind != ConnectivityPlanKind.Time || args.DurationMinutes is not null, nameof(DurationMinutes));
            Require(args.PlanKind != ConnectivityPlanKind.Data || args.IncludedDataMb is not null, nameof(IncludedDataMb));
            Require(args.PlanKind != ConnectivityPlanKind.FullFlight || args.DurationMinutes is null, nameof(DurationMinutes));
            Require(
                args.PlanKind != ConnectivityPlanKind.Messaging || args.DurationMinutes is not null || args.IncludedDataMb is not null,
                nameof(DurationMinutes));
            Require(
                args.DeliveryStage is PurchaseStage.PreOrder or PurchaseStage.PostTicketed or PurchaseStage.OnBoard,
                nameof(DeliveryStage));
            Require(
                args.FulfillmentProviderRef is null || SpecificationRules.IsCode(args.FulfillmentProviderRef.ToUpperInvariant(), ProviderRefMaxLength),
                nameof(FulfillmentProviderRef));

            var specification = new ConnectivitySpecification
            {
                PlanKind = args.PlanKind,
                DurationMinutes = args.DurationMinutes,
                IncludedDataMb = args.IncludedDataMb,
                MaxDevices = args.MaxDevices,
                DeliveryStage = args.DeliveryStage,
                FulfillmentProviderRef = args.FulfillmentProviderRef
            };

            specification._eligibleAircraft.AddRange(aircraft);

            return specification;
        }

        public ConnectivitySpecificationArgs ToArgs()
            => new(
                PlanKind,
                DurationMinutes,
                IncludedDataMb,
                MaxDevices,
                _eligibleAircraft.Select(row => (int)row.ReferenceId).ToList(),
                DeliveryStage,
                FulfillmentProviderRef);

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
