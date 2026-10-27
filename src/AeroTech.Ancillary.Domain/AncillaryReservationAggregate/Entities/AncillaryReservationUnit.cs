using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
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
        private const int VariantCodeMaxLength = 8;
        private const int ServiceDefinitionRefMaxLength = 30;
        private const string SelectionField = "Selection";

        private readonly List<AncillaryReservationUnitAllocation> _allocations = new();
        private readonly List<AncillaryReservationUnitFlight> _flights = new();

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
            var families = FamiliesOf(args.Selection);

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
            Require(args.VariantCode is { Length: >= 1 and <= VariantCodeMaxLength } && AncillaryVariant.Find(args.VariantCode) is not null, nameof(VariantCode));
            Require(args.ServiceDefinitionRef is { Length: >= 1 and <= ServiceDefinitionRefMaxLength }, nameof(ServiceDefinitionRef));
            Require(families.Count <= 1 && families.All(family => family == AncillaryVariant.Find(args.VariantCode)!.Profile), SelectionField);
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
                Require(families.Count == 0, SelectionField);
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
            VariantCode = args.VariantCode!;
            ServiceDefinitionRef = args.ServiceDefinitionRef!;
            _flights.AddRange(args.CoveredFlightIds.Order().Select(flightId => new AncillaryReservationUnitFlight(flightId)));
            Quantity = args.Quantity;
            SeatMapSelectionId = args.SeatMapSelectionId;
            SeatNumber = Normalize(args.SeatNumber);
            PricingId = args.PricingId;
            Baggage = args.Selection?.Baggage is { } baggage ? BaggageSelection.Create(baggage) : null;
            Meal = args.Selection?.Meal is { } meal ? MealSelection.Create(meal) : null;
            AssistedTravel = args.Selection?.AssistedTravel is { } assistedTravel ? AssistedTravelSelection.Create(assistedTravel) : null;
            AirportService = args.Selection?.AirportService is { } airportService ? AirportServiceSelection.Create(airportService) : null;
            Connectivity = args.Selection?.Connectivity is { } connectivity ? ConnectivitySelection.Create(connectivity) : null;
            Upgrade = args.Selection?.Upgrade is { } upgrade ? UpgradeSelection.Create(upgrade) : null;
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

        public string VariantCode { get; private set; } = default!;

        public string ServiceDefinitionRef { get; private set; } = default!;

        public IReadOnlyCollection<AncillaryReservationUnitFlight> Flights => _flights.AsReadOnly();

        public IReadOnlyList<long> CoveredFlightIds => _flights.Select(flight => flight.FlightId).Order().ToList();

        public int Quantity { get; private set; }

        public string? SeatMapSelectionId { get; private set; }

        public string? SeatNumber { get; private set; }

        public long? PricingId { get; private set; }

        public BaggageSelection? Baggage { get; private set; }

        public MealSelection? Meal { get; private set; }

        public AssistedTravelSelection? AssistedTravel { get; private set; }

        public AirportServiceSelection? AirportService { get; private set; }

        public ConnectivitySelection? Connectivity { get; private set; }

        public UpgradeSelection? Upgrade { get; private set; }

        public string? AppendIdempotencyKey { get; private set; }

        public long? StockPoolId { get; private set; }

        public string? ProviderUnitRef { get; private set; }

        public AncillaryReservationUnitStatus Status { get; private set; }

        public string? CancellationReasonCode { get; private set; }

        public IReadOnlyCollection<AncillaryReservationUnitAllocation> Allocations => _allocations.AsReadOnly();

        internal string? SeatKey => SeatNumber is null ? null : $"{string.Join(',', CoveredFlightIds)}|{SeatNumber}";

        internal static string? SeatKeyOf(AncillaryReservationUnitArgs args)
            => args.SeatNumber is null ? null : $"{string.Join(',', args.CoveredFlightIds.Order())}|{Normalize(args.SeatNumber)}";

        internal bool IsHeldBy(AncillaryReservationUnitArgs args)
            => OrderServiceId == args.OrderServiceId
               && ServiceDefinitionId == args.ServiceDefinitionId
               && ProvisionId == args.ProvisionId
               && TravellerId == args.TravellerId
               && CoverageScope == args.CoverageScope
               && CoveredFlightIds.SequenceEqual(args.CoveredFlightIds.Order())
               && Quantity == args.Quantity
               && string.Equals(SeatMapSelectionId, args.SeatMapSelectionId, StringComparison.Ordinal)
               && string.Equals(SeatNumber, Normalize(args.SeatNumber), StringComparison.Ordinal)
               && PricingId == args.PricingId
               && Baggage?.ToArgs() == args.Selection?.Baggage
               && Meal?.ToArgs() == args.Selection?.Meal
               && AssistedTravel?.ToArgs() == args.Selection?.AssistedTravel
               && AirportService?.ToArgs() == args.Selection?.AirportService
               && Connectivity?.ToArgs() == args.Selection?.Connectivity
               && Upgrade?.ToArgs() == args.Selection?.Upgrade;

        internal bool IsSelectedBy(AncillaryReservationSeatSelectionArgs seat)
            => OrderServiceId == seat.OrderServiceId
               && TravellerId == seat.TravellerId
               && string.Equals(SeatMapSelectionId, seat.SeatMapSelectionId, StringComparison.Ordinal);

        internal bool IsSelectedBy(AncillaryReservationServiceSelectionArgs service)
            => SeatNumber is null
               && VariantCode == service.VariantCode
               && AncillaryVariant.Find(VariantCode)?.Profile == service.Family
               && OrderServiceId == service.OrderServiceId
               && ProvisionId == service.ProvisionId
               && TravellerId == service.TravellerId
               && CoveredFlightIds.SequenceEqual(service.CoveredFlightIds.Order())
               && Quantity == (service.Quantity ?? Quantity)
               && (Baggage?.IsChosenBy(service.Options?.Baggage) ?? service.Options?.Baggage is null)
               && (AssistedTravel?.IsChosenBy(service.Options?.AssistedTravel) ?? service.Options?.AssistedTravel is null)
               && (AirportService?.IsChosenBy(service.Options?.AirportService) ?? service.Options?.AirportService is null)
               && (Connectivity?.IsChosenBy(service.Options?.Connectivity) ?? service.Options?.Connectivity is null);

        internal void Confirm() => Status = AncillaryReservationUnitStatus.Confirmed;

        internal void Release() => Status = AncillaryReservationUnitStatus.Released;

        internal void Expire() => Status = AncillaryReservationUnitStatus.Expired;

        internal void Cancel(string reasonCode)
        {
            Status = AncillaryReservationUnitStatus.Cancelled;
            CancellationReasonCode = reasonCode;
        }

        private static string? Normalize(string? seatNumber) => seatNumber?.Trim().ToUpperInvariant();

        private static IReadOnlyList<AncillaryProfile> FamiliesOf(AncillaryReservationSelectionArgs? selection)
            => selection is null
                ? []
                : new (AncillaryProfile Family, object? Part)[]
                    {
                        (AncillaryProfile.Baggage, selection.Baggage),
                        (AncillaryProfile.Meal, selection.Meal),
                        (AncillaryProfile.AssistedTravel, selection.AssistedTravel),
                        (AncillaryProfile.AirportService, selection.AirportService),
                        (AncillaryProfile.Connectivity, selection.Connectivity),
                        (AncillaryProfile.Upgrade, selection.Upgrade)
                    }
                    .Where(part => part.Part is not null)
                    .Select(part => part.Family)
                    .ToList();

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{nameof(AncillaryReservationUnit)}.{field}");
        }
    }
}
