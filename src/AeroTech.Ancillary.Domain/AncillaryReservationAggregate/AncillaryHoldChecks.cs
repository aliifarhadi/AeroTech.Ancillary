using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate
{
    public static class AncillaryHoldChecks
    {
        private static readonly IReadOnlySet<string> GatedVariants = new HashSet<string>(StringComparer.Ordinal)
        {
            AncillaryVariant.MedicalEquipment,
            AncillaryVariant.Bassinet,
            AncillaryVariant.UnaccompaniedMinor
        };

        public static void Accept(
            AncillaryReservationUnitArgs unit,
            AncillaryServiceDefinition? definition,
            AncillaryProvision? provision,
            Supplier? supplier)
        {
            if (definition is null)
                throw ExceptionFactory.AncillaryHoldServiceDefinitionNotFound();

            if (provision is null || provision.ServiceDefinitionId != definition.Id)
                throw ExceptionFactory.AncillaryHoldProvisionMismatch();

            if (supplier is null || supplier.Id != definition.SupplierId)
                throw ExceptionFactory.SupplierNotFound();

            if (supplier.FulfillmentKind == SupplierFulfillmentKind.External)
                throw ExceptionFactory.SupplierFulfillmentProviderNotRegistered(supplier.FulfillmentProviderKey);

            if (definition.Status != ServiceDefinitionStatus.Active || provision.Status != ProvisionStatus.Active)
                throw ExceptionFactory.AncillaryHoldNotReservable($"service {definition.Id} or its provision {provision.Id} is not active");

            EnsureHoldable(definition, provision);

            if (unit.Quantity < provision.Quantity.MinQuantity || unit.Quantity > provision.Quantity.MaxQuantity)
                throw ExceptionFactory.AncillaryHoldQuantityNotAllowed();

            if (unit.CoverageScope != provision.CoverageScope)
                throw ExceptionFactory.AncillaryHoldCoverageMismatch();
        }

        public static void EnsureHoldable(AncillaryServiceDefinition definition, AncillaryProvision provision)
        {
            if (definition.Booking.ConfirmationRequirement == ConfirmationRequirement.SubjectToConfirmation)
                throw ExceptionFactory.AncillaryHoldNotReservable($"service {definition.Id} needs a supplier confirmation that cannot be obtained");

            if (definition.VariantCode is { } variantCode && GatedVariants.Contains(variantCode))
                throw ExceptionFactory.AncillaryHoldNotReservable($"variant {variantCode} of service {definition.Id} cannot be held");

            if (provision.PriceOrigin == PriceOrigin.ExternalQuote)
                throw ExceptionFactory.AncillaryHoldNotReservable($"service {definition.Id} is priced by an external quote");
        }

        public static void EnsureReservable(
            AncillaryReservationUnitArgs unit,
            AncillaryServiceDefinition definition,
            AncillaryInventoryPolicy? policy)
        {
            if (policy is not { Status: InventoryRecordStatus.Active })
                throw ExceptionFactory.AncillaryHoldNotReservable($"service {definition.Id} has no active inventory policy");

            if (policy.PassengerUsageLimits.Count > 0)
                throw ExceptionFactory.AncillaryHoldNotReservable($"the passenger usage limits of service {definition.Id} cannot be proven");

            if (policy.Authority == InventoryAuthority.Supplier)
                throw ExceptionFactory.AncillaryHoldNotReservable($"the stock of service {definition.Id} is held by a supplier that is not connected");

            if (policy.Authority == InventoryAuthority.Local && policy is { CountConsumption: null, WeightConsumption: null, SlotConsumption: null })
                throw ExceptionFactory.InventoryPatternNotSupported(policy.LocalPattern);

            if (policy.WeightConsumption is { ConsumptionMode: FlightWeightConsumptionMode.AcceptedWeightKg })
                throw ExceptionFactory.AncillaryHoldNotReservable($"accepted-weight consumption of service {definition.Id} is not supported");

            if (policy.SlotConsumption is not null && unit.Selection?.AirportService?.TimeWithOffset is null)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(AirportServiceSelection.TimeWithOffset));

            if (policy.SlotConsumption is not null && definition.AirportService is null)
                throw ExceptionFactory.AncillaryHoldNotReservable($"the airport of service {definition.Id} is not stated");

            if ((policy.CountConsumption is not null || policy.WeightConsumption is not null) && unit.CoveredFlightIds.Count == 0)
                throw ExceptionFactory.AncillaryHoldNotReservable($"the flight stock of service {definition.Id} needs the covered flights");
        }

        public static void EnsureCovered(IReadOnlyList<AirportSlotInventory> slots, DateTimeOffset start, DateTimeOffset end)
        {
            var covered = start;

            foreach (var slot in slots.OrderBy(slot => slot.StartUtc))
            {
                if (slot.StartUtc <= covered && slot.EndUtc > covered)
                    covered = slot.EndUtc;
            }

            if (covered < end)
                throw ExceptionFactory.AncillaryHoldNotReservable($"the appointment {start:O} is not covered by contiguous airport slots");
        }

        public static void EnsureAvailable(
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            IReadOnlyDictionary<InventorySourceKey, InventorySourceBalance> balances)
        {
            var demands = units
                .SelectMany(unit => unit.Allocations ?? [])
                .GroupBy(allocation => new InventorySourceKey(allocation.ResourceKind, allocation.InventorySourceId))
                .Select(group => (Source: group.Key, Amount: group.Sum(allocation => allocation.Amount)));

            foreach (var (source, amount) in demands)
            {
                if (!balances.TryGetValue(source, out var balance) || balance.Status != InventoryRecordStatus.Active || balance.ClosedForSale)
                    throw ExceptionFactory.AncillaryHoldNotReservable($"inventory {source.ResourceKind} {source.InventorySourceId} is not open for sale");

                if (amount > balance.Capacity - balance.ActiveUsage)
                    throw ExceptionFactory.AncillaryHoldStockShortage(source.ResourceKind, source.InventorySourceId);
            }
        }
    }
}
