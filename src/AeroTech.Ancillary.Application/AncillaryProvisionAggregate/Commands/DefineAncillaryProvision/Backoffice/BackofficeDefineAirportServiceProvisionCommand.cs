using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed record BackofficeDefineAirportServiceProvisionCommand(
        long ServiceDefinitionId,
        int Sequence,
        ServiceCoverageScope CoverageScope,
        PurchaseStage PurchaseStage,
        PriceOrigin PriceOrigin,
        string? QuoteProviderKey,
        ProvisionQuantityInput Quantity,
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
        ProvisionAirportServiceRuleInput? AirportServiceRule) : IRequest<ProvisionResult>, IDefineAncillaryProvisionCommand
    {
        public AncillaryProfile? Profile => AncillaryProfile.AirportService;

        public ProvisionApplicationType ApplicationType => ProvisionApplicationType.Standard;

        public ProvisionBaggageApplicationInput? BaggageApplication => null;

        public ProvisionSeatApplicationInput? SeatApplication => null;

        public ProvisionPetRuleInput? PetRule => null;

        public ProvisionAssistedTravelRuleInput? AssistedTravelRule => null;
    }
}
