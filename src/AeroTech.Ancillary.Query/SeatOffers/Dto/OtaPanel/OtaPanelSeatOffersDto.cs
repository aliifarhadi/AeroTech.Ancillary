using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;

namespace AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel
{
    public sealed record OtaPanelSeatOffersDto(
        string OfferId,
        int CurrencyId,
        string? CurrencyCode,
        IReadOnlyList<OtaPanelSeatTravellerGroupDto> TravellerGroups,
        int BlockedCount);

    public sealed record OtaPanelSeatTravellerGroupDto(
        string TravellerRef,
        string PassengerTypeCode,
        IReadOnlyList<OtaPanelSeatFlightGroupDto> FlightGroups);

    public sealed record OtaPanelSeatFlightGroupDto(
        string FlightRef,
        string PortionRef,
        string? FlightNumber,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        IReadOnlyList<OtaPanelSeatOfferDto> SeatOffers);

    public sealed record OtaPanelSeatOfferDto(
        string OfferItemRef,
        string VariantCode,
        string Title,
        string? Description,
        string CoverageScope,
        IReadOnlyList<string> CoveredFlightRefs,
        string PriceStatus,
        string? PriceUnit,
        OtaPanelMoneyDto? UnitPrice,
        IReadOnlyList<OtaPanelFeeDto> OrderLevelFees,
        int MinQuantity,
        int MaxQuantity,
        IReadOnlyList<string> RequiredInputs,
        OtaPanelSeatTermsDto Seat,
        string DisplayStatus,
        bool CanPurchase);

    public sealed record OtaPanelSeatTermsDto(
        string Purpose,
        IReadOnlyList<string> CharacteristicCodes,
        IReadOnlyList<int> CabinIds,
        bool ExitRowEligibilityRequired,
        bool AdjacentSeatRequired,
        string? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        IReadOnlyList<string> SeatNumbers);

    public sealed record OtaPanelSeatOfferDetailDto(
        string OfferId,
        string TravellerRef,
        OtaPanelSeatOfferDto SeatOffer,
        OtaPanelPriceBreakdownDto PriceBreakdown,
        OtaPanelSeatSpecificationDto Specification,
        OtaPanelSeatRuleDto? SeatRule,
        OtaPanelRestrictionsDto Restrictions,
        OtaPanelTermsDto Terms,
        IReadOnlyList<OtaPanelRateDto> PriceRows);

    public sealed record OtaPanelSeatSelectionResultDto(
        string OfferId,
        string Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool SeatOfferChanged,
        OtaPanelSeatOfferDto SeatOffer,
        OtaPanelMoneyDto? SelectedAmount,
        IReadOnlyList<OtaPanelInputIssueDto> InputIssues,
        bool CanPurchase);
}
