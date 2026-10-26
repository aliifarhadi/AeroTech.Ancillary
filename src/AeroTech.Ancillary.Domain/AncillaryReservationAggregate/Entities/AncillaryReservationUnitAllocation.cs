using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities
{
    public sealed class AncillaryReservationUnitAllocation : Entity<long>
    {
        private AncillaryReservationUnitAllocation()
        {
        }

        internal AncillaryReservationUnitAllocation(long id, long ancillaryReservationUnitId, AncillaryReservationAllocationArgs args)
        {
            Require(Enum.IsDefined(args.ResourceKind), nameof(ResourceKind));
            Require(args.InventorySourceId > 0, nameof(InventorySourceId));
            Require(args.Amount > 0m && IsAmountOf(args.ResourceKind, args.Amount), nameof(Amount));

            Id = id;
            AncillaryReservationUnitId = ancillaryReservationUnitId;
            ResourceKind = args.ResourceKind;
            InventorySourceId = args.InventorySourceId;
            Amount = args.Amount;
        }

        public long AncillaryReservationUnitId { get; private set; }

        public InventoryResourceKind ResourceKind { get; private set; }

        public long InventorySourceId { get; private set; }

        public decimal Amount { get; private set; }

        private static bool IsAmountOf(InventoryResourceKind resourceKind, decimal amount)
            => resourceKind == InventoryResourceKind.FlightWeight
                ? InventoryRules.IsKg(amount)
                : decimal.Truncate(amount) == amount && amount <= int.MaxValue;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{nameof(AncillaryReservationUnitAllocation)}.{field}");
        }
    }
}
