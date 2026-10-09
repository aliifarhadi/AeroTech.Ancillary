using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services
{
    public sealed class InventoryPolicyEvidenceBuilder : IInventoryPolicyEvidenceBuilder
    {
        private readonly IInventoryCommercialFactsReader _facts;
        private readonly IFlightFlowDelegationReference _flightFlow;
        private readonly IInventoryResourceReference _resources;
        private readonly IAirportFacilityReference _facilities;
        private readonly ICountingFamilyReference _families;
        private readonly IFlightCountInventoryRepository _countInventories;

        public InventoryPolicyEvidenceBuilder(
            IInventoryCommercialFactsReader facts,
            IFlightFlowDelegationReference flightFlow,
            IInventoryResourceReference resources,
            IAirportFacilityReference facilities,
            ICountingFamilyReference families,
            IFlightCountInventoryRepository countInventories)
        {
            _facts = facts;
            _flightFlow = flightFlow;
            _resources = resources;
            _facilities = facilities;
            _families = families;
            _countInventories = countInventories;
        }

        public async Task<InventoryPolicyEvidence> BuildAsync(AncillaryInventoryPolicy policy, CancellationToken cancellationToken = default)
        {
            var owner = policy.OwnerAirlineId;
            var facts = await _facts.FindCurrentAsync(owner, policy.ServiceDefinitionRef, cancellationToken);
            var families = new Dictionary<string, InventoryReferenceCheck>(StringComparer.Ordinal);

            foreach (var limit in policy.PassengerUsageLimits)
                families[limit.CountingFamilyCode] = await _families.CheckAsync(owner, limit.CountingFamilyCode, cancellationToken);

            return new InventoryPolicyEvidence(
                facts?.ServiceDefinitionId,
                facts?.PricingUnit,
                facts?.ActiveExternalSupplierProviderKey,
                policy is { Authority: InventoryAuthority.FlightFlow, ProviderKey: not null }
                    ? await _flightFlow.CheckAsync(owner, policy.ProviderKey, cancellationToken)
                    : InventoryReferenceCheck.SourceUnavailable,
                policy.CountConsumption is { } count
                    ? await _resources.CheckAsync(owner, InventoryResourceKind.FlightCount, count.ResourceId, cancellationToken)
                    : InventoryReferenceCheck.SourceUnavailable,
                policy.CountConsumption is { } bound
                    ? await _countInventories.FindCountUnitOfLiveSourcesAsync(owner, bound.ResourceId, null, cancellationToken)
                    : null,
                policy.WeightConsumption is { } weight
                    ? await _resources.CheckAsync(owner, InventoryResourceKind.FlightWeight, weight.WeightResourceId, cancellationToken)
                    : InventoryReferenceCheck.SourceUnavailable,
                policy.SlotConsumption is { } slot
                    ? (await _facilities.CheckAsync(owner, slot.FacilityId, cancellationToken)).Result
                    : InventoryReferenceCheck.SourceUnavailable,
                families);
        }
    }
}
