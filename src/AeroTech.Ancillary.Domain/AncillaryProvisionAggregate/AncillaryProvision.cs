using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
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

        public DateTimeOffset? SalesEffectiveFrom { get; private set; }

        public DateTimeOffset? SalesDiscontinueAt { get; private set; }

        public ServiceCoverageScope CoverageScope { get; private set; }

        public AdvancePurchaseCriteria? AdvancePurchase { get; private set; }

        public QuantityRule Quantity { get; private set; } = default!;

        public ProvisionApplication Application { get; private set; } = default!;

        public CommercialOutcome Outcome { get; private set; } = default!;

        public SettlementDefinition Settlement { get; private set; } = default!;

        public AvailabilityDefinition Availability { get; private set; } = default!;

        public FulfillmentDefinition Fulfillment { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public static AncillaryProvision Define(
            long id,
            long serviceDefinitionId,
            int sequence,
            DateTimeOffset? salesEffectiveFrom,
            DateTimeOffset? salesDiscontinueAt,
            ServiceCoverageScope coverageScope,
            AdvancePurchaseCriteria? advancePurchase,
            QuantityRule quantity,
            ProvisionApplication application,
            CommercialOutcome outcome,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            ProvisionConditionsArgs conditions,
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
                advancePurchase,
                quantity,
                application,
                outcome,
                settlement,
                availability,
                fulfillment,
                conditions,
                idGenerator);

            return provision;
        }

        public void Change(
            int sequence,
            DateTimeOffset? salesEffectiveFrom,
            DateTimeOffset? salesDiscontinueAt,
            ServiceCoverageScope coverageScope,
            AdvancePurchaseCriteria? advancePurchase,
            QuantityRule quantity,
            ProvisionApplication application,
            CommercialOutcome outcome,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            ProvisionConditionsArgs conditions,
            IIdGenerator idGenerator)
        {
            EnsureDraft();

            Apply(
                sequence,
                salesEffectiveFrom,
                salesDiscontinueAt,
                coverageScope,
                advancePurchase,
                quantity,
                application,
                outcome,
                settlement,
                availability,
                fulfillment,
                conditions,
                idGenerator);
        }

        public void Activate(DateTimeOffset now)
        {
            EnsureDraft();

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
            AdvancePurchaseCriteria? advancePurchase,
            QuantityRule quantity,
            ProvisionApplication application,
            CommercialOutcome outcome,
            SettlementDefinition settlement,
            AvailabilityDefinition availability,
            FulfillmentDefinition fulfillment,
            ProvisionConditionsArgs conditions,
            IIdGenerator idGenerator)
        {
            Require(sequence > 0, nameof(Sequence));
            Require(
                salesEffectiveFrom is null || salesDiscontinueAt is null || salesEffectiveFrom < salesDiscontinueAt,
                nameof(SalesDiscontinueAt));
            Require(Enum.IsDefined(coverageScope), nameof(CoverageScope));

            var rows = Plan(conditions, idGenerator);

            EnsureSeatSelectors(application.Type, rows.SeatNumbers.Count, rows.SeatCharacteristics.Count, rows.Aircraft.Count);

            Sequence = sequence;
            SalesEffectiveFrom = salesEffectiveFrom;
            SalesDiscontinueAt = salesDiscontinueAt;
            CoverageScope = coverageScope;
            AdvancePurchase = advancePurchase;
            Quantity = quantity;
            Application = application;
            Outcome = outcome;
            Settlement = settlement;
            Availability = availability;
            Fulfillment = fulfillment;

            Commit(rows);
        }

        private void EnsureDraft()
        {
            if (Status != ProvisionStatus.Draft)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid(field);
        }
    }
}
