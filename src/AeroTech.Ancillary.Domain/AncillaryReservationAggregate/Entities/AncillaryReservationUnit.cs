using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities
{
    public sealed class AncillaryReservationUnit : Entity<long>
    {
        private const int SeatMapSelectionIdMaxLength = 256;
        private const int SeatNumberMaxLength = 16;

        private List<long> _coveredFlightIds = new();

        private AncillaryReservationUnit()
        {
        }

        internal AncillaryReservationUnit(long id, long ancillaryReservationId, AncillaryReservationUnitArgs args)
        {
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
            SeatNumber = args.SeatNumber?.Trim().ToUpperInvariant();
            PricingId = args.PricingId;
            Status = AncillaryReservationUnitStatus.Held;
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

        public long? StockPoolId { get; private set; }

        public string? ProviderUnitRef { get; private set; }

        public AncillaryReservationUnitStatus Status { get; private set; }

        public string? CancellationReasonCode { get; private set; }

        internal bool IsHeldBy(AncillaryReservationUnitArgs args)
            => OrderServiceId == args.OrderServiceId
               && ServiceDefinitionId == args.ServiceDefinitionId
               && ProvisionId == args.ProvisionId
               && TravellerId == args.TravellerId
               && CoverageScope == args.CoverageScope
               && _coveredFlightIds.SequenceEqual(args.CoveredFlightIds.Order())
               && Quantity == args.Quantity
               && string.Equals(SeatMapSelectionId, args.SeatMapSelectionId, StringComparison.Ordinal)
               && string.Equals(SeatNumber, args.SeatNumber?.Trim().ToUpperInvariant(), StringComparison.Ordinal)
               && PricingId == args.PricingId;

        internal void ChangeStatus(AncillaryReservationUnitStatus status) => Status = status;

        internal void Cancel(string reasonCode)
        {
            Status = AncillaryReservationUnitStatus.Cancelled;
            CancellationReasonCode = reasonCode;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{nameof(AncillaryReservationUnit)}.{field}");
        }
    }
}
