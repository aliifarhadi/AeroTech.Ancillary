using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed record EvaluationUnit(
        AncillaryShoppingContext Context,
        ShoppingTraveller Traveller,
        ServiceCoverageScope Scope,
        string CoverageRef,
        IReadOnlyList<ShoppingFlight> Flights,
        IReadOnlyList<ShoppingPortion> Portions,
        AncillarySelection? Selection)
    {
        public TravellerFareFacts? FareOf(ShoppingFlight flight)
            => Context.TravellerFareFacts.FirstOrDefault(fact => fact.TravellerRef == Traveller.TravellerRef && fact.FlightRef == flight.FlightRef);

        public TravellerFlightBaggageFacts? BaggageOf(ShoppingFlight flight)
            => Context.TravellerBaggageFacts.FirstOrDefault(fact => fact.TravellerRef == Traveller.TravellerRef && fact.FlightRef == flight.FlightRef);

        public IEnumerable<ExistingAncillaryServiceFacts> ActiveServices()
            => Context.ExistingServiceFacts.Where(service => service.TravellerRef == Traveller.TravellerRef
                                                             && service.CommercialState == ExistingServiceCommercialState.Active
                                                             && service.FlightRefs.Any(flightRef => Flights.Any(flight => flight.FlightRef == flightRef)));
    }
}
