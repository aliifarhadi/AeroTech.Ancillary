using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public sealed record AncillaryProvisionRuleRows(
        IReadOnlyList<AncillaryProvisionPassengerTypeReadModel> PassengerTypes,
        IReadOnlyList<AncillaryProvisionPointOfSaleReadModel> PointsOfSale,
        IReadOnlyList<AncillaryProvisionCustomerReadModel> Customers,
        IReadOnlyList<AncillaryProvisionCustomerTypeReadModel> CustomerTypes,
        IReadOnlyList<AncillaryProvisionOriginAirportReadModel> OriginAirports,
        IReadOnlyList<AncillaryProvisionDestinationAirportReadModel> DestinationAirports,
        IReadOnlyList<AncillaryProvisionViaAirportReadModel> ViaAirports,
        IReadOnlyList<AncillaryProvisionCoverageCountryReadModel> CoverageCountries,
        IReadOnlyList<AncillaryProvisionMarketingAirlineReadModel> MarketingAirlines,
        IReadOnlyList<AncillaryProvisionOperatingAirlineReadModel> OperatingAirlines,
        IReadOnlyList<AncillaryProvisionFlightNumberReadModel> FlightNumbers,
        IReadOnlyList<AncillaryProvisionFlightReadModel> Flights,
        IReadOnlyList<AncillaryProvisionAircraftReadModel> Aircraft,
        IReadOnlyList<AncillaryProvisionAirFareReadModel> AirFares,
        IReadOnlyList<AncillaryProvisionAirFareTypeReadModel> AirFareTypes,
        IReadOnlyList<AncillaryProvisionFareFamilyReadModel> FareFamilies,
        IReadOnlyList<AncillaryProvisionFareBasisReadModel> FareBases,
        IReadOnlyList<AncillaryProvisionCabinClassReadModel> CabinClasses,
        IReadOnlyList<AncillaryProvisionRbdReadModel> Rbds,
        IReadOnlyList<AncillaryProvisionSeatNumberReadModel> SeatNumbers,
        IReadOnlyList<AncillaryProvisionSeatCharacteristicReadModel> SeatCharacteristics,
        IReadOnlyList<AncillaryProvisionEligibleAgeBandReadModel> AgeBands,
        IReadOnlyList<AncillaryProvisionRoutePairReadModel> RoutePairs,
        IReadOnlyList<AncillaryProvisionServiceLocationReadModel> ServiceLocations,
        IReadOnlyList<AncillaryProvisionPermittedTravelPeriodReadModel> PermittedPeriods,
        IReadOnlyList<AncillaryProvisionBlackoutPeriodReadModel> BlackoutPeriods,
        IReadOnlyList<AncillaryProvisionDayTimeWindowReadModel> Windows);
}
