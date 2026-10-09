using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision
{
    public interface IChangeAncillaryProvisionCommand
    {
        long ProvisionId { get; }

        int Sequence { get; }

        ServiceCoverageScope CoverageScope { get; }

        PurchaseStage PurchaseStage { get; }

        ProvisionQuantityInput Quantity { get; }

        ProvisionApplicationType ApplicationType { get; }

        ProvisionOutcomeInput Outcome { get; }

        ProvisionSettlementInput Settlement { get; }

        ProvisionAvailabilityInput Availability { get; }

        ProvisionFulfillmentInput Fulfillment { get; }

        ProvisionPassengerEligibilityInput? PassengerEligibility { get; }

        ProvisionSalesRestrictionsInput? SalesRestrictions { get; }

        ProvisionGeographyInput? Geography { get; }

        ProvisionFlightApplicationInput? FlightApplication { get; }

        ProvisionFareApplicationInput? FareApplication { get; }

        ProvisionTravelDateInput? TravelDate { get; }

        ProvisionDayTimeApplicationInput? DayTimeApplication { get; }

        ProvisionAdvancePurchaseInput? AdvancePurchase { get; }

        ProvisionBaggageApplicationInput? BaggageApplication { get; }

        ProvisionSeatApplicationInput? SeatApplication { get; }
    }
}
