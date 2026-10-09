using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision : AggregateRoot<long>
    {
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

        public ServiceCoverageScope CoverageScope { get; private set; }

        public PurchaseStage PurchaseStage { get; private set; }

        public QuantityRule Quantity { get; private set; } = default!;

        public ProvisionApplicationType ApplicationType { get; private set; }

        public CommercialOutcome Outcome { get; private set; } = default!;

        public SettlementDefinition Settlement { get; private set; } = default!;

        public AvailabilityDefinition Availability { get; private set; } = default!;

        public FulfillmentDefinition Fulfillment { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public ProvisionPassengerEligibilityRule? PassengerEligibility { get; private set; }

        public ProvisionSalesRestrictionsRule? SalesRestrictions { get; private set; }

        public ProvisionGeographyRule? Geography { get; private set; }

        public ProvisionFlightApplicationRule? FlightApplication { get; private set; }

        public ProvisionFareApplicationRule? FareApplication { get; private set; }

        public ProvisionTravelDateRule? TravelDate { get; private set; }

        public ProvisionDayTimeApplicationRule? DayTimeApplication { get; private set; }

        public ProvisionAdvancePurchaseRule? AdvancePurchase { get; private set; }

        public ProvisionBaggageApplicationRule? BaggageApplication { get; private set; }

        public ProvisionSeatApplicationRule? SeatApplication { get; private set; }

        public static AncillaryProvision Define(
            long id,
            long serviceDefinitionId,
            int sequence,
            ServiceCoverageScope coverageScope,
            PurchaseStage purchaseStage,
            QuantityRule quantity,
            ProvisionApplicationType applicationType,
            CommercialOutcome outcome,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            ProvisionRulesArgs rules,
            IIdGenerator idGenerator,
            DateTimeOffset createdAt)
        {
            Require(serviceDefinitionId > 0, nameof(ServiceDefinitionId));

            var provision = new AncillaryProvision(id, serviceDefinitionId);

            provision.Status = ProvisionStatus.Draft;
            provision.CreatedAt = createdAt;
            provision.Apply(sequence, coverageScope, purchaseStage, quantity, applicationType, outcome, settlement, availability, fulfillment, rules, idGenerator);

            return provision;
        }

        public void Change(
            int sequence,
            ServiceCoverageScope coverageScope,
            PurchaseStage purchaseStage,
            QuantityRule quantity,
            ProvisionApplicationType applicationType,
            CommercialOutcome outcome,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            ProvisionRulesArgs rules,
            IIdGenerator idGenerator)
        {
            EnsureDraft();

            Apply(sequence, coverageScope, purchaseStage, quantity, applicationType, outcome, settlement, availability, fulfillment, rules, idGenerator);
        }

        public void Activate(AncillaryServiceDefinition definition, DateTimeOffset now)
        {
            EnsureDraft();
            EnsurePublishable(definition);
            EnsureDescriptorsAreStated();

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

        public void Reactivate(AncillaryServiceDefinition definition)
        {
            if (Status != ProvisionStatus.Suspended)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            EnsurePublishable(definition);

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
            ServiceCoverageScope coverageScope,
            PurchaseStage purchaseStage,
            QuantityRule quantity,
            ProvisionApplicationType applicationType,
            CommercialOutcome outcome,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            ProvisionRulesArgs rules,
            IIdGenerator idGenerator)
        {
            Require(sequence > 0, nameof(Sequence));
            Require(Enum.IsDefined(coverageScope), nameof(CoverageScope));
            Require(Enum.IsDefined(purchaseStage) && purchaseStage != PurchaseStage.LegacyUnspecified, nameof(PurchaseStage));
            Require(Enum.IsDefined(applicationType), nameof(ApplicationType));

            var passengerEligibility = ProvisionPassengerEligibilityRule.Plan(PassengerEligibility, Id, rules.PassengerEligibility, idGenerator);
            var salesRestrictions = ProvisionSalesRestrictionsRule.Plan(SalesRestrictions, Id, rules.SalesRestrictions, idGenerator);
            var geography = ProvisionGeographyRule.Plan(Geography, Id, rules.Geography, idGenerator);
            var flightApplication = ProvisionFlightApplicationRule.Plan(FlightApplication, Id, rules.FlightApplication, idGenerator);
            var fareApplication = ProvisionFareApplicationRule.Plan(FareApplication, Id, rules.FareApplication, idGenerator);
            var travelDate = ProvisionTravelDateRule.Plan(TravelDate, Id, rules.TravelDate, idGenerator);
            var dayTimeApplication = ProvisionDayTimeApplicationRule.Plan(DayTimeApplication, Id, rules.DayTimeApplication, idGenerator);
            var advancePurchase = ProvisionAdvancePurchaseRule.Plan(AdvancePurchase, Id, rules.AdvancePurchase, idGenerator);
            var baggageApplication = ProvisionBaggageApplicationRule.Plan(BaggageApplication, Id, rules.BaggageApplication, idGenerator);
            var seatApplication = ProvisionSeatApplicationRule.Plan(SeatApplication, Id, rules.SeatApplication, idGenerator);

            EnsureApplicationRules(
                applicationType,
                rules.BaggageApplication is not null,
                rules.SeatApplication?.SeatNumbers.Count ?? 0,
                rules.SeatApplication?.SeatCharacteristicCodes.Count ?? 0,
                rules.FlightApplication?.AircraftIds.Count ?? 0);

            Sequence = sequence;
            CoverageScope = coverageScope;
            PurchaseStage = purchaseStage;
            Quantity = quantity;
            ApplicationType = applicationType;
            Outcome = outcome;
            Settlement = settlement;
            Availability = availability;
            Fulfillment = fulfillment;

            PassengerEligibility = passengerEligibility();
            SalesRestrictions = salesRestrictions();
            Geography = geography();
            FlightApplication = flightApplication();
            FareApplication = fareApplication();
            TravelDate = travelDate();
            DayTimeApplication = dayTimeApplication();
            AdvancePurchase = advancePurchase();
            BaggageApplication = baggageApplication();
            SeatApplication = seatApplication();
        }

        private void EnsureDraft()
        {
            if (Status != ProvisionStatus.Draft)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();
        }

        private static void EnsureApplicationRules(
            ProvisionApplicationType applicationType,
            bool baggage,
            int seatNumbers,
            int seatCharacteristics,
            int aircraft)
        {
            Require(baggage == (applicationType == ProvisionApplicationType.Baggage), nameof(BaggageApplication));
            Require(
                applicationType == ProvisionApplicationType.Seat ? seatNumbers + seatCharacteristics > 0 : seatNumbers + seatCharacteristics == 0,
                nameof(SeatApplication));
            Require(seatNumbers == 0 || aircraft > 0, nameof(FlightApplication));
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid(field);
        }
    }
}
