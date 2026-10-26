using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed class AncillaryHoldAllocator : IAncillaryHoldAllocator
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly ISupplierRepository _suppliers;
        private readonly IInventoryPolicyRepository _policies;
        private readonly IFlightCountInventoryRepository _counts;
        private readonly IFlightWeightInventoryRepository _weights;
        private readonly IAirportSlotInventoryRepository _slots;

        public AncillaryHoldAllocator(
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryProvisionRepository provisions,
            ISupplierRepository suppliers,
            IInventoryPolicyRepository policies,
            IFlightCountInventoryRepository counts,
            IFlightWeightInventoryRepository weights,
            IAirportSlotInventoryRepository slots)
        {
            _definitions = definitions;
            _provisions = provisions;
            _suppliers = suppliers;
            _policies = policies;
            _counts = counts;
            _weights = weights;
            _slots = slots;
        }

        public async Task<IReadOnlyList<AncillaryReservationUnitArgs>> AllocateAsync(
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            CancellationToken cancellationToken = default)
        {
            var allocated = new List<AncillaryReservationUnitArgs>(units.Count);

            foreach (var unit in units)
                allocated.Add(await AllocateAsync(unit, cancellationToken));

            return allocated;
        }

        private async Task<AncillaryReservationUnitArgs> AllocateAsync(AncillaryReservationUnitArgs unit, CancellationToken cancellationToken)
        {
            var definition = await _definitions.GetAsync(unit.ServiceDefinitionId, cancellationToken);
            var provision = await _provisions.GetAsync(unit.ProvisionId, cancellationToken);
            var supplier = definition is null ? null : await _suppliers.GetAsync(definition.SupplierId, cancellationToken);

            AncillaryHoldChecks.Accept(unit, definition, provision, supplier);

            if (unit.SeatNumber is not null)
                return unit;

            var policy = await _policies.FindActiveAsync(definition!.OwnerAirlineId, definition.ServiceDefinitionRef, cancellationToken);

            AncillaryHoldChecks.EnsureReservable(unit, definition, policy);

            return unit with { Allocations = await AllocationsAsync(unit, definition, policy!, cancellationToken) };
        }

        private async Task<IReadOnlyList<AncillaryReservationAllocationArgs>> AllocationsAsync(
            AncillaryReservationUnitArgs unit,
            AncillaryServiceDefinition definition,
            AncillaryInventoryPolicy policy,
            CancellationToken cancellationToken)
        {
            var allocations = new List<AncillaryReservationAllocationArgs>();

            foreach (var flightId in unit.CoveredFlightIds)
            {
                if (policy.CountConsumption is { } count)
                    allocations.Add(new AncillaryReservationAllocationArgs(
                        InventoryResourceKind.FlightCount,
                        await _counts.FindCurrentIdAsync(definition.OwnerAirlineId, flightId, count.ResourceId, cancellationToken)
                        ?? throw ExceptionFactory.AncillaryHoldNotReservable($"flight {flightId} has no count inventory for resource {count.ResourceId}"),
                        count.RequiredUnits(unit.Quantity)));

                if (policy.WeightConsumption is { } weight)
                    allocations.Add(new AncillaryReservationAllocationArgs(
                        InventoryResourceKind.FlightWeight,
                        await _weights.FindCurrentIdAsync(definition.OwnerAirlineId, flightId, weight.WeightResourceId, cancellationToken)
                        ?? throw ExceptionFactory.AncillaryHoldNotReservable($"flight {flightId} has no weight inventory for resource {weight.WeightResourceId}"),
                        weight.RequiredKg(unit.Quantity, null)));
            }

            if (policy.SlotConsumption is { } slot)
            {
                var start = unit.AppointmentTimeWithOffset!.Value;
                var end = start.AddMinutes(slot.OccupancyMinutes);
                var slots = await _slots.ListCurrentAsync(definition.OwnerAirlineId, definition.AirportService!.AirportId, slot.FacilityId, start, end, cancellationToken);
                var persons = slot.RequiredPersons(unit.Quantity, unit.GuestCount ?? 0);

                AncillaryHoldChecks.EnsureCovered(slots, start, end);
                allocations.AddRange(slots.Select(covering => new AncillaryReservationAllocationArgs(InventoryResourceKind.AirportSlot, covering.Id, persons)));
            }

            return allocations;
        }
    }
}
