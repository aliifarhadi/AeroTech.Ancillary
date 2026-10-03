using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceReservationAggregate.Entities
{
    public sealed class ServiceReservationUnit : Entity<long>
    {
        private const int UnitReferenceMaxLength = 128;

        private List<long> _coveredFlightIds = new();

        private ServiceReservationUnit()
        {
        }

        internal ServiceReservationUnit(long id, long serviceReservationId, ServiceReservationUnitArgs args)
        {
            if (args.UnitReference is not { Length: >= 1 and <= UnitReferenceMaxLength })
                throw ExceptionFactory.ServiceReservationRequestIsInvalid($"{nameof(ServiceReservationUnit)}.{nameof(UnitReference)}");

            if (args.CoveredFlightIds.Count == 0)
                throw ExceptionFactory.ServiceReservationRequestIsInvalid($"{nameof(ServiceReservationUnit)}.{nameof(CoveredFlightIds)}");

            if (args.Quantity < 1)
                throw ExceptionFactory.ServiceReservationRequestIsInvalid($"{nameof(ServiceReservationUnit)}.{nameof(Quantity)}");

            Id = id;
            ServiceReservationId = serviceReservationId;
            UnitReference = args.UnitReference;
            OwnerAirlineId = args.OwnerAirlineId;
            ProductRef = args.ProductRef;
            ProductVersion = args.ProductVersion;
            PriceRuleId = args.PriceRuleId;
            TravellerRef = args.TravellerRef;
            BoundRef = args.BoundRef;
            FlightRef = args.FlightRef;
            _coveredFlightIds = args.CoveredFlightIds.ToList();
            Quantity = args.Quantity;
            CurrencyId = args.CurrencyId;
            Total = args.Total;
            InventoryControl = args.InventoryControl;
            Status = ServiceReservationUnitStatus.Held;
        }

        public long ServiceReservationId { get; private set; }

        public string UnitReference { get; private set; } = default!;

        public int OwnerAirlineId { get; private set; }

        public string ProductRef { get; private set; } = default!;

        public int ProductVersion { get; private set; }

        public long PriceRuleId { get; private set; }

        public string TravellerRef { get; private set; } = default!;

        public string BoundRef { get; private set; } = default!;

        public string? FlightRef { get; private set; }

        public IReadOnlyList<long> CoveredFlightIds => _coveredFlightIds.AsReadOnly();

        public int Quantity { get; private set; }

        public int CurrencyId { get; private set; }

        public decimal Total { get; private set; }

        public AncillaryInventoryControl InventoryControl { get; private set; }

        public ServiceReservationUnitStatus Status { get; private set; }

        internal bool IsSelectedBy(ServiceReservationUnitSelection selection)
            => selection.UnitReference == UnitReference
               && selection.ProductRef == ProductRef
               && selection.ProductVersion == ProductVersion
               && selection.PriceRuleId == PriceRuleId
               && selection.TravellerRef == TravellerRef
               && selection.FlightRef == FlightRef
               && (selection.BoundRef == BoundRef || (FlightRef is not null && selection.BoundRef is null))
               && selection.Quantity == Quantity;

        internal void ChangeStatus(ServiceReservationUnitStatus status) => Status = status;
    }
}
