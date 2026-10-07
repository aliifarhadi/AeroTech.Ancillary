using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed class AncillaryProvision : AggregateRoot<long>
    {
        private static readonly FeeApplicationUnit[] ImplementedFeeApplicationUnits =
        [
            FeeApplicationUnit.OneWay,
            FeeApplicationUnit.RoundTrip,
            FeeApplicationUnit.Item,
            FeeApplicationUnit.SectorOrPortion,
            FeeApplicationUnit.Ticket
        ];

        private readonly List<ProvisionRoutePair> _routePairs = new();
        private readonly List<ProvisionPriceLine> _priceLines = new();

        private AncillaryProvision()
        {
        }

        private AncillaryProvision(long id, long serviceDefinitionId)
        {
            Id = id;
            ServiceDefinitionId = serviceDefinitionId;
        }

        public long ServiceDefinitionId { get; private set; }

        public int Sequence { get; private set; }

        public ProvisionStatus Status { get; private set; }

        public DateTimeOffset? SalesEffectiveFrom { get; private set; }

        public DateTimeOffset? SalesDiscontinueAt { get; private set; }

        public ServiceCoverageScope CoverageScope { get; private set; }

        public PassengerCriteria Passenger { get; private set; } = default!;

        public SalesCriteria Sales { get; private set; } = default!;

        public TravelCriteria Travel { get; private set; } = default!;

        public FareCriteria Fare { get; private set; } = default!;

        public AdvancePurchaseCriteria? AdvancePurchase { get; private set; }

        public QuantityRule Quantity { get; private set; } = default!;

        public ProvisionApplication Application { get; private set; } = default!;

        public CommercialOutcome Outcome { get; private set; } = default!;

        public FeeDefinition? Fee { get; private set; }

        public SettlementDefinition Settlement { get; private set; } = default!;

        public AvailabilityDefinition Availability { get; private set; } = default!;

        public FulfillmentDefinition Fulfillment { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public IReadOnlyCollection<ProvisionRoutePair> RoutePairs => _routePairs.AsReadOnly();

        public IReadOnlyCollection<ProvisionPriceLine> PriceLines => _priceLines.AsReadOnly();

        public static AncillaryProvision Define(
            long id,
            long serviceDefinitionId,
            int sequence,
            DateTimeOffset? salesEffectiveFrom,
            DateTimeOffset? salesDiscontinueAt,
            ServiceCoverageScope coverageScope,
            PassengerCriteria passenger,
            SalesCriteria sales,
            TravelCriteria travel,
            IReadOnlyList<ProvisionRoutePairArgs> routePairs,
            FareCriteria fare,
            AdvancePurchaseCriteria? advancePurchase,
            QuantityRule quantity,
            ProvisionApplication application,
            CommercialOutcome outcome,
            FeeDefinition? fee,
            IReadOnlyList<ProvisionPriceLineArgs> priceLines,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            IIdGenerator idGenerator,
            DateTimeOffset createdAt)
        {
            Require(serviceDefinitionId > 0, nameof(ServiceDefinitionId));

            var provision = new AncillaryProvision(id, serviceDefinitionId);

            provision.Status = ProvisionStatus.Draft;
            provision.CreatedAt = createdAt;
            provision.Apply(
                sequence,
                salesEffectiveFrom,
                salesDiscontinueAt,
                coverageScope,
                passenger,
                sales,
                travel,
                routePairs,
                fare,
                advancePurchase,
                quantity,
                application,
                outcome,
                fee,
                priceLines,
                settlement,
                availability,
                fulfillment,
                idGenerator);

            return provision;
        }

        public void Change(
            int sequence,
            DateTimeOffset? salesEffectiveFrom,
            DateTimeOffset? salesDiscontinueAt,
            ServiceCoverageScope coverageScope,
            PassengerCriteria passenger,
            SalesCriteria sales,
            TravelCriteria travel,
            IReadOnlyList<ProvisionRoutePairArgs> routePairs,
            FareCriteria fare,
            AdvancePurchaseCriteria? advancePurchase,
            QuantityRule quantity,
            ProvisionApplication application,
            CommercialOutcome outcome,
            FeeDefinition? fee,
            IReadOnlyList<ProvisionPriceLineArgs> priceLines,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            IIdGenerator idGenerator)
        {
            if (Status != ProvisionStatus.Draft)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            Apply(
                sequence,
                salesEffectiveFrom,
                salesDiscontinueAt,
                coverageScope,
                passenger,
                sales,
                travel,
                routePairs,
                fare,
                advancePurchase,
                quantity,
                application,
                outcome,
                fee,
                priceLines,
                settlement,
                availability,
                fulfillment,
                idGenerator);
        }

        public void Activate(DateTimeOffset now)
        {
            if (Status != ProvisionStatus.Draft)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            if (Fee is { } fee && !ImplementedFeeApplicationUnits.Contains(fee.ApplicationUnit))
                throw ExceptionFactory.ProvisionFeeApplicationUnitNotSupported(fee.ApplicationUnit);

            Status = ProvisionStatus.Active;
            ActivatedAt = now;
        }

        public void Supersede(DateTimeOffset now)
        {
            if (Status != ProvisionStatus.Active)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            Status = ProvisionStatus.Retired;
            RetiredAt = now;
        }

        public void Suspend(DateTimeOffset now)
        {
            if (Status != ProvisionStatus.Active)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            Status = ProvisionStatus.Suspended;
            SuspendedAt = now;
        }

        public void Reactivate()
        {
            if (Status != ProvisionStatus.Suspended)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            Status = ProvisionStatus.Active;
            SuspendedAt = null;
        }

        public void Retire(DateTimeOffset now)
        {
            if (Status == ProvisionStatus.Retired)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            Status = ProvisionStatus.Retired;
            RetiredAt = now;
        }

        private void Apply(
            int sequence,
            DateTimeOffset? salesEffectiveFrom,
            DateTimeOffset? salesDiscontinueAt,
            ServiceCoverageScope coverageScope,
            PassengerCriteria passenger,
            SalesCriteria sales,
            TravelCriteria travel,
            IReadOnlyList<ProvisionRoutePairArgs> routePairs,
            FareCriteria fare,
            AdvancePurchaseCriteria? advancePurchase,
            QuantityRule quantity,
            ProvisionApplication application,
            CommercialOutcome outcome,
            FeeDefinition? fee,
            IReadOnlyList<ProvisionPriceLineArgs> priceLines,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            IIdGenerator idGenerator)
        {
            Require(sequence > 0, nameof(Sequence));
            Require(
                salesEffectiveFrom is null || salesDiscontinueAt is null || salesEffectiveFrom < salesDiscontinueAt,
                nameof(SalesDiscontinueAt));
            Require(Enum.IsDefined(coverageScope), nameof(CoverageScope));
            Require(
                routePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Distinct().Count() == routePairs.Count,
                nameof(RoutePairs));
            Require(
                application.Seat is null || application.Seat.SeatNumbers.Count == 0 || travel.AircraftIds.Count > 0,
                $"{nameof(TravelCriteria)}.{nameof(TravelCriteria.AircraftIds)}");

            if (outcome.Disposition == CommercialDisposition.Paid)
            {
                Require(fee is not null, nameof(Fee));
                Require(priceLines.Count > 0, nameof(PriceLines));
            }
            else
            {
                Require(fee is null, nameof(Fee));
                Require(priceLines.Count == 0, nameof(PriceLines));
            }

            var newRoutePairs = routePairs.Select(pair => new ProvisionRoutePair(idGenerator.NewId(), Id, pair)).ToList();
            var newPriceLines = priceLines.Select(line => new ProvisionPriceLine(idGenerator.NewId(), Id, line)).ToList();

            Sequence = sequence;
            SalesEffectiveFrom = salesEffectiveFrom;
            SalesDiscontinueAt = salesDiscontinueAt;
            CoverageScope = coverageScope;
            Passenger = passenger;
            Sales = sales;
            Travel = travel;
            Fare = fare;
            AdvancePurchase = advancePurchase;
            Quantity = quantity;
            Application = application;
            Outcome = outcome;
            Fee = fee;
            Settlement = settlement;
            Availability = availability;
            Fulfillment = fulfillment;

            _routePairs.Clear();
            _routePairs.AddRange(newRoutePairs);
            _priceLines.Clear();
            _priceLines.AddRange(newPriceLines);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid(field);
        }
    }
}
