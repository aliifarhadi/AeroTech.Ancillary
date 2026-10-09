using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record DefineProvisionRequest(
        long ServiceDefinitionId,
        int Sequence,
        ServiceCoverageScope CoverageScope,
        PurchaseStage PurchaseStage,
        PriceOrigin PriceOrigin,
        string? QuoteProviderKey,
        ProvisionQuantityInput Quantity,
        ProvisionApplicationType ApplicationType,
        ProvisionOutcomeInput Outcome,
        ProvisionSettlementInput Settlement,
        ProvisionAvailabilityInput Availability,
        ProvisionFulfillmentInput Fulfillment,
        ProvisionPassengerEligibilityInput? PassengerEligibility,
        ProvisionSalesRestrictionsInput? SalesRestrictions,
        ProvisionGeographyInput? Geography,
        ProvisionFlightApplicationInput? FlightApplication,
        ProvisionFareApplicationInput? FareApplication,
        ProvisionTravelDateInput? TravelDate,
        ProvisionDayTimeApplicationInput? DayTimeApplication,
        ProvisionAdvancePurchaseInput? AdvancePurchase,
        ProvisionBaggageApplicationInput? BaggageApplication,
        ProvisionSeatApplicationInput? SeatApplication,
        ProvisionPetRuleInput? PetRule,
        ProvisionAssistedTravelRuleInput? AssistedTravelRule,
        ProvisionAirportServiceRuleInput? AirportServiceRule);
}
