using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot
{
    public sealed class GetInventoryConfigurationSnapshotService : IGetInventoryConfigurationSnapshotService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;
        private readonly IInventoryCommercialFactsReader _facts;
        private readonly IClock _clock;

        public GetInventoryConfigurationSnapshotService(
            AncillaryQueryDbContext dbContext,
            IInventoryCallerScope scope,
            IInventoryCommercialFactsReader facts,
            IClock clock)
        {
            _dbContext = dbContext;
            _scope = scope;
            _facts = facts;
            _clock = clock;
        }

        public async Task<InventoryConfigurationSnapshotDto> ExecuteAsync(IGetInventoryConfigurationSnapshotQuery query, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var snapshot = await ResolveAsync(ownerAirlineId, query, cancellationToken);
            var current = await _facts.FindCurrentAsync(ownerAirlineId, query.ServiceDefinitionRef, cancellationToken);
            var requiresAvailabilityCheck = current is not null
                                            && await _dbContext.AncillaryProvisions
                                                .AsNoTracking()
                                                .AnyAsync(
                                                    provision => provision.ServiceDefinitionId == current.ServiceDefinitionId
                                                                 && provision.Status == ProvisionStatus.Active
                                                                 && provision.MustCheckAvailability,
                                                    cancellationToken);

            return snapshot with { CurrentServiceDefinitionId = current?.ServiceDefinitionId, RequiresAvailabilityCheck = requiresAvailabilityCheck };
        }

        private async Task<InventoryConfigurationSnapshotDto> ResolveAsync(
            int ownerAirlineId,
            IGetInventoryConfigurationSnapshotQuery query,
            CancellationToken cancellationToken)
        {
            var policy = await _dbContext.AncillaryInventoryPolicies
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    row => row.OwnerAirlineId == ownerAirlineId
                           && row.ServiceDefinitionRef == query.ServiceDefinitionRef
                           && row.Status != InventoryRecordStatus.Retired,
                    cancellationToken);

            if (policy is null)
                return Snapshot(ownerAirlineId, query.ServiceDefinitionRef, null, InventoryCapacityReadState.NotConfigured, "PolicyNotConfigured");

            if (policy.Status == InventoryRecordStatus.Draft)
            {
                return policy.LocalPattern is LocalInventoryPattern.DailyCount or LocalInventoryPattern.RoomNight or LocalInventoryPattern.AssignedAsset
                    ? Snapshot(ownerAirlineId, query.ServiceDefinitionRef, policy, InventoryCapacityReadState.UnsupportedPattern, "PatternNotSupported")
                    : Snapshot(ownerAirlineId, query.ServiceDefinitionRef, policy, InventoryCapacityReadState.NotConfigured, "PolicyNotActive");
            }

            var suspended = policy.Status == InventoryRecordStatus.Suspended;

            if (policy.Authority == InventoryAuthority.Unlimited)
            {
                return suspended
                    ? Snapshot(ownerAirlineId, query.ServiceDefinitionRef, policy, InventoryCapacityReadState.ClosedForSale, "PolicySuspended", closedForSale: true)
                    : Snapshot(ownerAirlineId, query.ServiceDefinitionRef, policy, InventoryCapacityReadState.Unlimited, null);
            }

            if (policy.Authority != InventoryAuthority.Local)
            {
                return suspended
                    ? Snapshot(ownerAirlineId, query.ServiceDefinitionRef, policy, InventoryCapacityReadState.ClosedForSale, "PolicySuspended", closedForSale: true)
                    : Snapshot(ownerAirlineId, query.ServiceDefinitionRef, policy, InventoryCapacityReadState.DelegatedCheckRequired, "DelegatedSourceNotConnected");
            }

            return policy.LocalPattern == LocalInventoryPattern.AirportSlot
                ? await SlotSnapshotAsync(policy, query, suspended, cancellationToken)
                : await FlightSnapshotAsync(policy, query, suspended, cancellationToken);
        }

        private async Task<InventoryConfigurationSnapshotDto> FlightSnapshotAsync(
            InventoryPolicyReadModel policy,
            IGetInventoryConfigurationSnapshotQuery query,
            bool suspended,
            CancellationToken cancellationToken)
        {
            var kind = policy.LocalPattern switch
            {
                LocalInventoryPattern.FlightCount => (InventoryResourceKind?)InventoryResourceKind.FlightCount,
                LocalInventoryPattern.FlightWeight => InventoryResourceKind.FlightWeight,
                _ => null
            };

            if (query.FlightId is not { } flightId)
            {
                return Snapshot(
                    policy.OwnerAirlineId,
                    policy.ServiceDefinitionRef,
                    policy,
                    InventoryCapacityReadState.Unknown,
                    "FlightRequired",
                    kind,
                    new InventoryResourceLocatorDto(null, policy.CountResourceId, policy.WeightResourceId, null, null, null, null));
            }

            var locator = new InventoryResourceLocatorDto(flightId, policy.CountResourceId, policy.WeightResourceId, null, null, null, null);
            var count = policy.CountResourceId is { } countResourceId
                ? await _dbContext.FlightCountInventories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        row => row.OwnerAirlineId == policy.OwnerAirlineId
                               && row.FlightId == flightId
                               && row.ResourceId == countResourceId
                               && row.Status != InventoryRecordStatus.Retired,
                        cancellationToken)
                : null;
            var weight = policy.WeightResourceId is { } weightResourceId
                ? await _dbContext.FlightWeightInventories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        row => row.OwnerAirlineId == policy.OwnerAirlineId
                               && row.FlightId == flightId
                               && row.WeightResourceId == weightResourceId
                               && row.Status != InventoryRecordStatus.Retired,
                        cancellationToken)
                : null;
            var countMissing = policy.CountResourceId is not null && (count is null || count.Status == InventoryRecordStatus.Draft);
            var weightMissing = policy.WeightResourceId is not null && (weight is null || weight.Status == InventoryRecordStatus.Draft);

            if (countMissing || weightMissing)
                return Snapshot(policy.OwnerAirlineId, policy.ServiceDefinitionRef, policy, InventoryCapacityReadState.NotConfigured, "SourceNotConfigured", kind, locator);

            var closed = suspended
                         || count is { ClosedForSale: true } or { Status: InventoryRecordStatus.Suspended }
                         || weight is { ClosedForSale: true } or { Status: InventoryRecordStatus.Suspended };

            return Snapshot(
                policy.OwnerAirlineId,
                policy.ServiceDefinitionRef,
                policy,
                closed ? InventoryCapacityReadState.ClosedForSale : InventoryCapacityReadState.ConfiguredNotGuaranteed,
                closed ? (suspended ? "PolicySuspended" : "SourceClosedForSale") : "NoAllocationLedger",
                kind,
                locator,
                count?.TotalCapacity,
                weight?.CapacityKg,
                closed);
        }

        private async Task<InventoryConfigurationSnapshotDto> SlotSnapshotAsync(
            InventoryPolicyReadModel policy,
            IGetInventoryConfigurationSnapshotQuery query,
            bool suspended,
            CancellationToken cancellationToken)
        {
            var facilityId = policy.SlotFacilityId!.Value;

            if (query.AtUtc is not { } atUtc)
            {
                return Snapshot(
                    policy.OwnerAirlineId,
                    policy.ServiceDefinitionRef,
                    policy,
                    InventoryCapacityReadState.Unknown,
                    "InstantRequired",
                    InventoryResourceKind.AirportSlot,
                    new InventoryResourceLocatorDto(null, null, null, null, facilityId, null, null));
            }

            var slot = await _dbContext.AirportSlotInventories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    row => row.OwnerAirlineId == policy.OwnerAirlineId
                           && row.FacilityId == facilityId
                           && row.Status != InventoryRecordStatus.Retired
                           && row.StartUtc <= atUtc
                           && atUtc < row.EndUtc,
                    cancellationToken);

            if (slot is null || slot.Status == InventoryRecordStatus.Draft)
            {
                return Snapshot(
                    policy.OwnerAirlineId,
                    policy.ServiceDefinitionRef,
                    policy,
                    InventoryCapacityReadState.NotConfigured,
                    "SourceNotConfigured",
                    InventoryResourceKind.AirportSlot,
                    new InventoryResourceLocatorDto(null, null, null, null, facilityId, null, null));
            }

            var closed = suspended || slot.ClosedForSale || slot.Status == InventoryRecordStatus.Suspended;

            return Snapshot(
                policy.OwnerAirlineId,
                policy.ServiceDefinitionRef,
                policy,
                closed ? InventoryCapacityReadState.ClosedForSale : InventoryCapacityReadState.ConfiguredNotGuaranteed,
                closed ? (suspended ? "PolicySuspended" : "SourceClosedForSale") : "NoAllocationLedger",
                InventoryResourceKind.AirportSlot,
                new InventoryResourceLocatorDto(null, null, null, slot.AirportId, facilityId, slot.StartUtc, slot.EndUtc),
                slot.CapacityPersons,
                null,
                closed);
        }

        private InventoryConfigurationSnapshotDto Snapshot(
            int ownerAirlineId,
            string serviceDefinitionRef,
            InventoryPolicyReadModel? policy,
            InventoryCapacityReadState state,
            string? reasonCode,
            InventoryResourceKind? resourceKind = null,
            InventoryResourceLocatorDto? resource = null,
            int? configuredCount = null,
            decimal? configuredKg = null,
            bool? closedForSale = null)
            => new(
                policy?.Id,
                ownerAirlineId,
                serviceDefinitionRef,
                null,
                policy is null ? null : EnumValueDto.Of(policy.Authority),
                policy?.LocalPattern is { } pattern ? EnumValueDto.Of(pattern) : null,
                resourceKind is { } kind ? EnumValueDto.Of(kind) : null,
                resource,
                configuredCount,
                configuredKg,
                closedForSale,
                EnumValueDto.Of(state),
                _clock.GetDateTime(),
                null,
                false,
                false,
                reasonCode);
    }
}
