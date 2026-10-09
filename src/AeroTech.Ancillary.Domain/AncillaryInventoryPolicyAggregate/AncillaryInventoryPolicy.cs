using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate
{
    public sealed class AncillaryInventoryPolicy : AggregateRoot<long>
    {
        private const int ServiceDefinitionRefMaxLength = 30;
        private const int ProviderKeyMaxLength = 50;

        private readonly List<PassengerUsageLimit> _passengerUsageLimits = new();

        private AncillaryInventoryPolicy()
        {
        }

        private AncillaryInventoryPolicy(long id, int ownerAirlineId, string serviceDefinitionRef, DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            ServiceDefinitionRef = serviceDefinitionRef;
            Status = InventoryRecordStatus.Draft;
            Version = 1;
            CreatedAt = createdAt;
            UpdatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public string ServiceDefinitionRef { get; private set; } = default!;

        public long ServiceDefinitionId { get; private set; }

        public InventoryAuthority Authority { get; private set; }

        public LocalInventoryPattern? LocalPattern { get; private set; }

        public string? ProviderKey { get; private set; }

        public FlightCountConsumption? CountConsumption { get; private set; }

        public FlightWeightConsumption? WeightConsumption { get; private set; }

        public AirportSlotConsumption? SlotConsumption { get; private set; }

        public IReadOnlyCollection<PassengerUsageLimit> PassengerUsageLimits => _passengerUsageLimits.AsReadOnly();

        public InventoryRecordStatus Status { get; private set; }

        public long Version { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public static AncillaryInventoryPolicy Define(
            long id,
            int ownerAirlineId,
            string serviceDefinitionRef,
            InventoryPolicyArgs args,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(InventoryRules.IsCode(serviceDefinitionRef, ServiceDefinitionRefMaxLength), nameof(ServiceDefinitionRef));

            var policy = new AncillaryInventoryPolicy(id, ownerAirlineId, serviceDefinitionRef, now);

            policy.Apply(args, idGenerator);

            return policy;
        }

        public void Change(InventoryPolicyArgs args, long expectedVersion, IIdGenerator idGenerator, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Draft);
            EnsureVersion(expectedVersion);
            Apply(args, idGenerator);
            Touch(now);
        }

        public void Activate(InventoryPolicyEvidence evidence, long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Draft, InventoryRecordStatus.Suspended);
            EnsureVersion(expectedVersion);

            if (!evidence.ServiceDefinitionMatchesIdentity)
                throw ExceptionFactory.InventoryPolicyServiceDefinitionMismatch();

            switch (Authority)
            {
                case InventoryAuthority.Unlimited:
                    EnsureUnlimitedIsCredible(evidence);
                    break;
                case InventoryAuthority.Supplier:
                    EnsureSupplierIsRecorded(evidence);
                    break;
                case InventoryAuthority.FlightFlow:
                    EnsureFlightFlowIsDelegated(evidence);
                    break;
                default:
                    EnsureLocalIsComplete(evidence);
                    break;
            }

            foreach (var limit in _passengerUsageLimits)
            {
                InventoryRules.EnsureVerified(
                    evidence.CountingFamilies.GetValueOrDefault(limit.CountingFamilyCode, InventoryReferenceCheck.SourceUnavailable),
                    $"Counting family {limit.CountingFamilyCode}");
            }

            Status = InventoryRecordStatus.Active;
            ActivatedAt ??= now;
            SuspendedAt = null;
            Touch(now);
        }

        public void Suspend(long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Active);
            EnsureVersion(expectedVersion);
            Status = InventoryRecordStatus.Suspended;
            SuspendedAt = now;
            Touch(now);
        }

        public void Retire(long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Draft, InventoryRecordStatus.Active, InventoryRecordStatus.Suspended);
            EnsureVersion(expectedVersion);
            Status = InventoryRecordStatus.Retired;
            RetiredAt = now;
            Touch(now);
        }

        private void Apply(InventoryPolicyArgs args, IIdGenerator idGenerator)
        {
            Require(args.ServiceDefinitionId > 0, nameof(ServiceDefinitionId));
            Require(Enum.IsDefined(args.Authority), nameof(Authority));
            Require(args.ProviderKey is null || InventoryRules.IsCode(args.ProviderKey, ProviderKeyMaxLength), nameof(ProviderKey));

            if (args.Authority == InventoryAuthority.Local)
            {
                Require(args.LocalPattern is not null && Enum.IsDefined(args.LocalPattern.Value), nameof(LocalPattern));
                Require(args.ProviderKey is null, nameof(ProviderKey));
                Require(args.CountConsumption is null || Allows(args.LocalPattern!.Value, InventoryResourceKind.FlightCount), nameof(CountConsumption));
                Require(args.WeightConsumption is null || Allows(args.LocalPattern!.Value, InventoryResourceKind.FlightWeight), nameof(WeightConsumption));
                Require(args.SlotConsumption is null || Allows(args.LocalPattern!.Value, InventoryResourceKind.AirportSlot), nameof(SlotConsumption));
            }
            else
            {
                Require(args.LocalPattern is null, nameof(LocalPattern));
                Require(args.CountConsumption is null, nameof(CountConsumption));
                Require(args.WeightConsumption is null, nameof(WeightConsumption));
                Require(args.SlotConsumption is null, nameof(SlotConsumption));
                Require(args.Authority != InventoryAuthority.Unlimited || args.ProviderKey is null, nameof(ProviderKey));
            }

            var limits = PlanLimits(args.PassengerUsageLimits, idGenerator);

            ServiceDefinitionId = args.ServiceDefinitionId;
            Authority = args.Authority;
            LocalPattern = args.LocalPattern;
            ProviderKey = args.ProviderKey;
            CountConsumption = args.CountConsumption?.Copy();
            WeightConsumption = args.WeightConsumption?.Copy();
            SlotConsumption = args.SlotConsumption?.Copy();
            _passengerUsageLimits.RemoveAll(limit => !limits.Contains(limit));

            foreach (var limit in limits.Where(limit => !_passengerUsageLimits.Contains(limit)))
                _passengerUsageLimits.Add(limit);
        }

        private List<PassengerUsageLimit> PlanLimits(IReadOnlyList<PassengerUsageLimitArgs> requested, IIdGenerator idGenerator)
        {
            Require(requested.All(limit => limit is not null && Enum.IsDefined(limit.LimitScope)), nameof(PassengerUsageLimits));
            Require(requested.Select(limit => limit.LimitScope).Distinct().Count() == requested.Count, nameof(PassengerUsageLimits));

            var planned = new List<PassengerUsageLimit>();
            var changes = new List<Action>();

            foreach (var args in requested)
            {
                var stored = _passengerUsageLimits.FirstOrDefault(limit => limit.LimitScope == args.LimitScope);

                if (stored is null)
                {
                    planned.Add(new PassengerUsageLimit(idGenerator.NewId(), Id, args));
                    continue;
                }

                Require(args.MaxUnits > 0, nameof(PassengerUsageLimit.MaxUnits));
                Require(
                    InventoryRules.IsCode(PassengerUsageLimit.Normalize(args.CountingFamilyCode), PassengerUsageLimit.CountingFamilyCodeMaxLength),
                    nameof(PassengerUsageLimit.CountingFamilyCode));
                planned.Add(stored);
                changes.Add(() => stored.Change(args));
            }

            changes.ForEach(change => change());

            return planned;
        }

        private void EnsureUnlimitedIsCredible(InventoryPolicyEvidence evidence)
        {
            if (evidence.ActiveProvisionMustCheckAvailability)
                throw ExceptionFactory.InventoryPolicyActivationRefused("an active provision of this service must check availability, so it cannot be declared unlimited");
        }

        private void EnsureSupplierIsRecorded(InventoryPolicyEvidence evidence)
        {
            if (ProviderKey is null)
                throw ExceptionFactory.InventoryPolicyActivationRefused("a supplier-managed policy needs the provider key of the supplier");

            if (!string.Equals(evidence.SupplierProviderKey, ProviderKey, StringComparison.Ordinal))
                throw ExceptionFactory.InventoryPolicyActivationRefused("the provider key is not the recorded key of the active external supplier of this service");
        }

        private void EnsureFlightFlowIsDelegated(InventoryPolicyEvidence evidence)
        {
            if (ProviderKey is null)
                throw ExceptionFactory.InventoryPolicyActivationRefused("a FlightFlow-managed policy needs the delegated provider key");

            InventoryRules.EnsureVerified(evidence.FlightFlowDelegation, "FlightFlow delegated source");
        }

        private void EnsureLocalIsComplete(InventoryPolicyEvidence evidence)
        {
            var pattern = LocalPattern!.Value;

            if (pattern is LocalInventoryPattern.DailyCount or LocalInventoryPattern.RoomNight or LocalInventoryPattern.AssignedAsset)
                throw ExceptionFactory.InventoryPatternNotSupported(pattern);

            var needsCount = Allows(pattern, InventoryResourceKind.FlightCount);
            var needsWeight = Allows(pattern, InventoryResourceKind.FlightWeight);
            var needsSlot = Allows(pattern, InventoryResourceKind.AirportSlot);

            if ((needsCount && CountConsumption is null) || (needsWeight && WeightConsumption is null) || (needsSlot && SlotConsumption is null))
                throw ExceptionFactory.InventoryPolicyActivationRefused($"pattern {pattern} needs exactly one binding of each of its resources");

            if (needsCount)
                InventoryRules.EnsureVerified(evidence.CountResource, $"Count resource {CountConsumption!.ResourceId}");

            if (needsWeight)
                InventoryRules.EnsureVerified(evidence.WeightResource, $"Weight resource {WeightConsumption!.WeightResourceId}");

            if (needsSlot)
                InventoryRules.EnsureVerified(evidence.SlotFacility, $"Airport facility {SlotConsumption!.FacilityId}");

            EnsureUnitsAlign(evidence, needsCount, needsWeight);
        }

        private void EnsureUnitsAlign(InventoryPolicyEvidence evidence, bool needsCount, bool needsWeight)
        {
            if (evidence.PricingUnit is null)
                throw ExceptionFactory.InventoryUnitMismatch("the service has no pricing unit");

            var perKilogram = evidence.PricingUnit == PricingUnit.PerKilogram;

            if (needsWeight && WeightConsumption!.ConsumptionMode == FlightWeightConsumptionMode.AcceptedWeightKg && !perKilogram)
                throw ExceptionFactory.InventoryUnitMismatch("accepted-weight consumption needs a service priced per kilogram");

            if (needsWeight && WeightConsumption!.ConsumptionMode == FlightWeightConsumptionMode.FixedKgPerAcceptedUnit && perKilogram)
                throw ExceptionFactory.InventoryUnitMismatch("a service priced per kilogram consumes its accepted weight, not a fixed weight per unit");

            if (needsCount && !needsWeight && perKilogram)
                throw ExceptionFactory.InventoryUnitMismatch("a service priced per kilogram needs a weight binding");

            if (needsCount && evidence.CountUnitOfLiveSources is { } unit && unit != CountConsumption!.CountUnit)
                throw ExceptionFactory.InventoryUnitMismatch($"resource {CountConsumption.ResourceId} is counted in {unit}");
        }

        private static bool Allows(LocalInventoryPattern pattern, InventoryResourceKind resourceKind)
            => resourceKind switch
            {
                InventoryResourceKind.FlightCount => pattern is LocalInventoryPattern.FlightCount or LocalInventoryPattern.FlightCountPlusWeight,
                InventoryResourceKind.FlightWeight => pattern is LocalInventoryPattern.FlightWeight or LocalInventoryPattern.FlightCountPlusWeight,
                _ => pattern is LocalInventoryPattern.AirportSlot
            };

        private void EnsureStatus(params InventoryRecordStatus[] allowed)
        {
            if (!allowed.Contains(Status))
                throw ExceptionFactory.InventoryPolicyStatusChangeNotAllowed();
        }

        private void EnsureVersion(long expectedVersion)
        {
            if (expectedVersion != Version)
                throw ExceptionFactory.InventoryVersionConflict();
        }

        private void Touch(DateTimeOffset now)
        {
            Version++;
            UpdatedAt = now;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.InventoryPolicyIsInvalid($"{nameof(AncillaryInventoryPolicy)}.{field}");
        }
    }
}
