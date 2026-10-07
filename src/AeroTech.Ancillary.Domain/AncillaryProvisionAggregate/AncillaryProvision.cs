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

        private readonly List<ProvisionPriceLine> _priceLines = new();

        private AncillaryProvision()
        {
        }

        private AncillaryProvision(long id, long serviceDefinitionId, int sequence)
        {
            Id = id;
            ServiceDefinitionId = serviceDefinitionId;
            Sequence = sequence;
        }

        public long ServiceDefinitionId { get; private set; }

        public int Sequence { get; private set; }

        public ProvisionStatus Status { get; private set; }

        public DateTimeOffset? SalesEffectiveFrom { get; private set; }

        public DateTimeOffset? SalesDiscontinueAt { get; private set; }

        public ServiceCoverageScope CoverageScope { get; private set; }

        public QuantityRule Quantity { get; private set; } = default!;

        public ProvisionApplicationType ApplicationType { get; private set; }

        public CommercialOutcome Outcome { get; private set; } = default!;

        public FeeDefinition? Fee { get; private set; }

        public SettlementDefinition Settlement { get; private set; } = default!;

        public AvailabilityDefinition Availability { get; private set; } = default!;

        public FulfillmentDefinition Fulfillment { get; private set; } = default!;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public IReadOnlyCollection<ProvisionPriceLine> PriceLines => _priceLines.AsReadOnly();

        public static AncillaryProvision Define(
            long id,
            long serviceDefinitionId,
            int sequence,
            DateTimeOffset? salesEffectiveFrom,
            DateTimeOffset? salesDiscontinueAt,
            ServiceCoverageScope coverageScope,
            QuantityRule quantity,
            ProvisionApplicationType applicationType,
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
            Require(sequence > 0, nameof(Sequence));
            Require(
                salesEffectiveFrom is null || salesDiscontinueAt is null || salesEffectiveFrom < salesDiscontinueAt,
                nameof(SalesDiscontinueAt));
            Require(Enum.IsDefined(coverageScope), nameof(CoverageScope));
            Require(Enum.IsDefined(applicationType), nameof(ApplicationType));

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

            var provision = new AncillaryProvision(id, serviceDefinitionId, sequence);

            provision.SalesEffectiveFrom = salesEffectiveFrom;
            provision.SalesDiscontinueAt = salesDiscontinueAt;
            provision.CoverageScope = coverageScope;
            provision.Quantity = quantity;
            provision.ApplicationType = applicationType;
            provision.Outcome = outcome;
            provision.Fee = fee;
            provision.Settlement = settlement;
            provision.Availability = availability;
            provision.Fulfillment = fulfillment;
            provision.Status = ProvisionStatus.Draft;
            provision.CreatedAt = createdAt;
            provision._priceLines.AddRange(priceLines.Select(line => new ProvisionPriceLine(idGenerator.NewId(), id, line)));

            return provision;
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

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid(field);
        }
    }
}
