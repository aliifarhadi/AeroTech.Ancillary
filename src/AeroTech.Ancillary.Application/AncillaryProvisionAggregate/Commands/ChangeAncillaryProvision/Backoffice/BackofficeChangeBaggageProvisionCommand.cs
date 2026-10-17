using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed record BackofficeChangeBaggageProvisionCommand(
        long? ServiceDefinitionId,
        long ProvisionId,
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
        ProvisionBaggageApplicationInput? BaggageApplication) : IRequest<ProvisionResult>, IChangeAncillaryProvisionCommand
    {
        public AncillaryProfile? Profile => AncillaryProfile.Baggage;

        public ProvisionApplicationType ApplicationType => ProvisionApplicationType.Baggage;

        public ProvisionSeatApplicationInput? SeatApplication => null;

        public ProvisionPetRuleInput? PetRule => null;

        public ProvisionAssistedTravelRuleInput? AssistedTravelRule => null;

        public ProvisionAirportServiceRuleInput? AirportServiceRule => null;
    }
}
