namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionRulesArgs(
        ProvisionPassengerEligibilityArgs? PassengerEligibility,
        ProvisionSalesRestrictionsArgs? SalesRestrictions,
        ProvisionGeographyArgs? Geography,
        ProvisionFlightApplicationArgs? FlightApplication,
        ProvisionFareApplicationArgs? FareApplication,
        ProvisionTravelDateArgs? TravelDate,
        ProvisionDayTimeApplicationArgs? DayTimeApplication,
        ProvisionAdvancePurchaseArgs? AdvancePurchase,
        ProvisionBaggageApplicationArgs? BaggageApplication,
        ProvisionSeatApplicationArgs? SeatApplication,
        ProvisionPetRuleArgs? PetRule = null,
        ProvisionAssistedTravelRuleArgs? AssistedTravelRule = null,
        ProvisionAirportServiceRuleArgs? AirportServiceRule = null,
        IReadOnlyList<ProvisionCancellationRuleArgs>? CancellationRules = null)
    {
        public static ProvisionRulesArgs Unrestricted { get; } = new(null, null, null, null, null, null, null, null, null, null);
    }
}
