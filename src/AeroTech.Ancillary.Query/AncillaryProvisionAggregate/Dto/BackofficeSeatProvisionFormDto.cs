using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeSeatProvisionFormDto(
        long Id,
        long ServiceDefinitionId,
        EnumValueDto Status,
        EnumValueDto Profile,
        string VariantCode,
        bool Editable,
        IReadOnlyList<string> AllowedRuleSections,
        SeatProvisionConstraintsDto Constraints,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        SeatProvisionFormDto Form);

    public sealed record SeatProvisionFormDto(
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
        ProvisionSeatApplicationInput? SeatApplication);
}
