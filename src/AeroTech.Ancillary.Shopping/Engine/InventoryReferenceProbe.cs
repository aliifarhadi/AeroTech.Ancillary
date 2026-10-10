using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed class InventoryReferenceProbe
    {
        private readonly IInventoryResourceReference _resources;
        private readonly IAirportFacilityReference _facilities;
        private readonly ICountingFamilyReference _families;
        private readonly IFlightFlowDelegationReference _flightFlow;
        private readonly int _ownerAirlineId;
        private readonly Dictionary<string, InventoryReferenceCheck> _answers = new(StringComparer.Ordinal);

        public InventoryReferenceProbe(
            IInventoryResourceReference resources,
            IAirportFacilityReference facilities,
            ICountingFamilyReference families,
            IFlightFlowDelegationReference flightFlow,
            int ownerAirlineId)
        {
            _resources = resources;
            _facilities = facilities;
            _families = families;
            _flightFlow = flightFlow;
            _ownerAirlineId = ownerAirlineId;
        }

        public Task<InventoryReferenceCheck> ResourceAsync(InventoryResourceKind kind, long resourceId, CancellationToken cancellationToken)
            => OnceAsync($"R:{kind}:{resourceId}", () => _resources.CheckAsync(_ownerAirlineId, kind, resourceId, cancellationToken));

        public Task<InventoryReferenceCheck> FacilityAsync(long facilityId, CancellationToken cancellationToken)
            => OnceAsync($"F:{facilityId}", async () => (await _facilities.CheckAsync(_ownerAirlineId, facilityId, cancellationToken)).Result);

        public Task<InventoryReferenceCheck> FamilyAsync(string countingFamilyCode, CancellationToken cancellationToken)
            => OnceAsync($"C:{countingFamilyCode}", () => _families.CheckAsync(_ownerAirlineId, countingFamilyCode, cancellationToken));

        public Task<InventoryReferenceCheck> FlightFlowAsync(string providerKey, CancellationToken cancellationToken)
            => OnceAsync($"D:{providerKey}", () => _flightFlow.CheckAsync(_ownerAirlineId, providerKey, cancellationToken));

        private async Task<InventoryReferenceCheck> OnceAsync(string key, Func<Task<InventoryReferenceCheck>> check)
        {
            if (!_answers.TryGetValue(key, out var answer))
                _answers[key] = answer = await check();

            return answer;
        }
    }
}
