using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities
{
    public sealed class AncillaryReservationUnit : Entity<long>
    {
        private const int SeatMapSelectionIdMaxLength = 256;
        private const int SeatNumberMaxLength = 16;
        private const int AppendIdempotencyKeyMaxLength = 128;

        private readonly List<AncillaryReservationUnitAllocation> _allocations = new();
        private List<long> _coveredFlightIds = new();

        private AncillaryReservationUnit()
        {
        }

        internal AncillaryReservationUnit(
            long id,
            long ancillaryReservationId,
            AncillaryReservationUnitArgs args,
            string? appendIdempotencyKey,
            IIdGenerator idGenerator)
        {
            var allocations = args.Allocations ?? [];

            Require(args.OrderServiceId > 0, nameof(OrderServiceId));
            Require(args.ServiceDefinitionId > 0, nameof(ServiceDefinitionId));
            Require(args.ProvisionId > 0, nameof(ProvisionId));
            Require(Enum.IsDefined(args.CoverageScope), nameof(CoverageScope));
            Require(args.Quantity >= 1, nameof(Quantity));
            Require(args.CoveredFlightIds.Distinct().Count() == args.CoveredFlightIds.Count, nameof(CoveredFlightIds));
            Require(args.SeatMapSelectionId is null or { Length: >= 1 and <= SeatMapSelectionIdMaxLength }, nameof(SeatMapSelectionId));
            Require(args.SeatNumber is null or { Length: >= 1 and <= SeatNumberMaxLength }, nameof(SeatNumber));
            Require((args.SeatMapSelectionId is null) == (args.SeatNumber is null) && (args.SeatNumber is null) == (args.PricingId is null), nameof(SeatMapSelectionId));
            Require(args.PricingId is null or > 0, nameof(PricingId));
            Require(args.GuestCount is null or >= 0, nameof(GuestCount));
            Require(args.GuestCount is null || args.AppointmentTimeWithOffset is not null, nameof(GuestCount));
            Require(appendIdempotencyKey is null or { Length: >= 1 and <= AppendIdempotencyKeyMaxLength }, nameof(AppendIdempotencyKey));
            Require(allocations.Select(allocation => (allocation.ResourceKind, allocation.InventorySourceId)).Distinct().Count() == allocations.Count, nameof(Allocations));

            if (args.CoverageScope == ServiceCoverageScope.Order)
            {
                Require(args.TravellerId is null, nameof(TravellerId));
            }
            else
            {
                Require(args.TravellerId is > 0, nameof(TravellerId));
                Require(args.CoveredFlightIds.Count >= 1, nameof(CoveredFlightIds));
            }

            if (args.CoverageScope == ServiceCoverageScope.Sector)
                Require(args.CoveredFlightIds.Count == 1, nameof(CoveredFlightIds));

            if (args.SeatNumber is not null)
            {
                Require(args.Quantity == 1, nameof(Quantity));
                Require(args.CoverageScope == ServiceCoverageScope.Sector, nameof(CoverageScope));
                Require(args.AppointmentTimeWithOffset is null, nameof(AppointmentTimeWithOffset));
                Require(allocations.Count == 0, nameof(Allocations));
            }
            else
            {
                Require(appendIdempotencyKey is null, nameof(AppendIdempotencyKey));
            }

            Id = id;
            AncillaryReservationId = ancillaryReservationId;
            OrderServiceId = args.OrderServiceId;
            ServiceDefinitionId = args.ServiceDefinitionId;
            ProvisionId = args.ProvisionId;
            TravellerId = args.TravellerId;
            CoverageScope = args.CoverageScope;
            _coveredFlightIds = args.CoveredFlightIds.Order().ToList();
            Quantity = args.Quantity;
            SeatMapSelectionId = args.SeatMapSelectionId;
            SeatNumber = Normalize(args.SeatNumber);
            PricingId = args.PricingId;
            AppointmentTimeWithOffset = args.AppointmentTimeWithOffset;
            GuestCount = args.GuestCount;
            AppendIdempotencyKey = appendIdempotencyKey;
            Status = AncillaryReservationUnitStatus.Held;
            _allocations.AddRange(allocations.Select(allocation => new AncillaryReservationUnitAllocation(idGenerator.NewId(), id, allocation)));
        }

        public long AncillaryReservationId { get; private set; }

        public long OrderServiceId { get; private set; }

        public long ServiceDefinitionId { get; private set; }

        public long ProvisionId { get; private set; }

        public long? TravellerId { get; private set; }

        public ServiceCoverageScope CoverageScope { get; private set; }

        public IReadOnlyList<long> CoveredFlightIds => _coveredFlightIds.AsReadOnly();

        public int Quantity { get; private set; }

        public string? SeatMapSelectionId { get; private set; }

        public string? SeatNumber { get; private set; }

        public long? PricingId { get; private set; }

        public DateTimeOffset? AppointmentTimeWithOffset { get; private set; }

        public int? GuestCount { get; private set; }

        public string? AppendIdempotencyKey { get; private set; }

        public long? StockPoolId { get; private set; }

        public string? ProviderUnitRef { get; private set; }

        public AncillaryReservationUnitStatus Status { get; private set; }

        public string? CancellationReasonCode { get; private set; }

        public IReadOnlyCollection<AncillaryReservationUnitAllocation> Allocations => _allocations.AsReadOnly();

        internal string? SeatKey => SeatNumber is null ? null : $"{string.Join(',', _coveredFlightIds)}|{SeatNumber}";

        internal static string? SeatKeyOf(AncillaryReservationUnitArgs args)
            => args.SeatNumber is null ? null : $"{string.Join(',', args.CoveredFlightIds.Order())}|{Normalize(args.SeatNumber)}";

        internal bool IsHeldBy(AncillaryReservationUnitArgs args)
            => OrderServiceId == args.OrderServiceId
               && ServiceDefinitionId == args.ServiceDefinitionId
               && ProvisionId == args.ProvisionId
               && TravellerId == args.TravellerId
               && CoverageScope == args.CoverageScope
               && _coveredFlightIds.SequenceEqual(args.CoveredFlightIds.Order())
               && Quantity == args.Quantity
               && string.Equals(SeatMapSelectionId, args.SeatMapSelectionId, StringComparison.Ordinal)
               && string.Equals(SeatNumber, Normalize(args.SeatNumber), StringComparison.Ordinal)
               && PricingId == args.PricingId
               && AppointmentTimeWithOffset == args.AppointmentTimeWithOffset
               && GuestCount == args.GuestCount;

        internal bool IsSelectedBy(AncillaryReservationSeatSelectionArgs seat)
            => OrderServiceId == seat.OrderServiceId
               && TravellerId == seat.TravellerId
               && string.Equals(SeatMapSelectionId, seat.SeatMapSelectionId, StringComparison.Ordinal);

        internal void Confirm() => Status = AncillaryReservationUnitStatus.Confirmed;

        internal void Release() => Status = AncillaryReservationUnitStatus.Released;

        internal void Expire() => Status = AncillaryReservationUnitStatus.Expired;

        internal void Cancel(string reasonCode)
        {
            Status = AncillaryReservationUnitStatus.Cancelled;
            CancellationReasonCode = reasonCode;
        }

        private static string? Normalize(string? seatNumber) => seatNumber?.Trim().ToUpperInvariant();

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{nameof(AncillaryReservationUnit)}.{field}");
        }
    }
}
