using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed record BackofficeChangePetProvisionCommand(
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
        ProvisionPetRuleInput? PetRule) : IRequest<ProvisionResult>, IChangeAncillaryProvisionCommand
    {
        public AncillaryProfile? Profile => AncillaryProfile.Pet;

        public ProvisionApplicationType ApplicationType => ProvisionApplicationType.Standard;

        public ProvisionBaggageApplicationInput? BaggageApplication => null;

        public ProvisionSeatApplicationInput? SeatApplication => null;

        public ProvisionAssistedTravelRuleInput? AssistedTravelRule => null;

        public ProvisionAirportServiceRuleInput? AirportServiceRule => null;
    }
}
