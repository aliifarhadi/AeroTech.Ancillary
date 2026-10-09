using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class InventoryFixture
    : ICallerContext,
        IOperatorAirlineResolver,
        IFlightOccurrenceReference,
        IInventoryResourceReference,
        IAirportFacilityReference,
        IFlightFlowDelegationReference,
        ICountingFamilyReference,
        IAirportReference
{
    public InventoryFixture(int homeAirlineId) => HomeAirlineId = homeAirlineId;

    public long? HomeAirlineId { get; set; }

    public CallerContextType ContextType { get; set; } = CallerContextType.Airline;

    public CallerPrincipalType PrincipalType { get; set; } = CallerPrincipalType.Human;

    public long ActorId { get; set; } = 42;

    public long? AirlineUserId => ActorId;

    public long? AirlineOfficeId => null;

    public long? TravelAgencyUserId => null;

    public long? TravelAgencyId => null;

    public IReadOnlyCollection<long> TravelAgencyOfficeIds => [];

    public long? IndividualId => null;

    public long? PartnerApiAccessProfileId => null;

    public long? CustomerId => null;

    public HashSet<long>? Flights { get; set; }

    public HashSet<(InventoryResourceKind Kind, long Id)>? Resources { get; set; }

    public Dictionary<long, (int AirportId, string TimeZoneId)>? Facilities { get; set; }

    public HashSet<string>? FlightFlowProviderKeys { get; set; }

    public HashSet<string>? CountingFamilies { get; set; }

    public HashSet<int> Airports { get; } = [];

    public static InventoryFixture Connected(int homeAirlineId)
        => new(homeAirlineId)
        {
            Flights = [],
            Resources = [],
            Facilities = [],
            FlightFlowProviderKeys = [],
            CountingFamilies = []
        };

    public Task<long?> FindHomeAirlineIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(HomeAirlineId);

    Task<InventoryReferenceCheck> IFlightOccurrenceReference.CheckAsync(int ownerAirlineId, long flightId, CancellationToken cancellationToken)
        => Task.FromResult(Check(Flights, flightId));

    Task<InventoryReferenceCheck> IInventoryResourceReference.CheckAsync(
        int ownerAirlineId,
        InventoryResourceKind resourceKind,
        long resourceId,
        CancellationToken cancellationToken)
        => Task.FromResult(Check(Resources, (resourceKind, resourceId)));

    Task<AirportFacilityCheck> IAirportFacilityReference.CheckAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken)
        => Task.FromResult(
            Facilities is null
                ? new AirportFacilityCheck(InventoryReferenceCheck.SourceUnavailable, null, null)
                : Facilities.TryGetValue(facilityId, out var facility)
                    ? new AirportFacilityCheck(InventoryReferenceCheck.Verified, facility.AirportId, facility.TimeZoneId)
                    : new AirportFacilityCheck(InventoryReferenceCheck.NotFound, null, null));

    Task<InventoryReferenceCheck> IFlightFlowDelegationReference.CheckAsync(int ownerAirlineId, string providerKey, CancellationToken cancellationToken)
        => Task.FromResult(Check(FlightFlowProviderKeys, providerKey));

    Task<InventoryReferenceCheck> ICountingFamilyReference.CheckAsync(int ownerAirlineId, string countingFamilyCode, CancellationToken cancellationToken)
        => Task.FromResult(Check(CountingFamilies, countingFamilyCode));

    public Task<bool> ExistsAsync(int airportId, CancellationToken cancellationToken = default) => Task.FromResult(Airports.Contains(airportId));

    private static InventoryReferenceCheck Check<TValue>(HashSet<TValue>? known, TValue value)
        => known is null
            ? InventoryReferenceCheck.SourceUnavailable
            : known.Contains(value) ? InventoryReferenceCheck.Verified : InventoryReferenceCheck.NotFound;
}
