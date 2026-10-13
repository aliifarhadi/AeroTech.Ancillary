using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice
{
    public sealed record BackofficeSeatOffersDto(
        string OfferId,
        DateTimeOffset EvaluatedAtUtc,
        OfferCurrencyDto Currency,
        FlightOfferSummaryDto FlightOffer,
        OfferScopeDto Scope,
        OfferJourneyDto Journey,
        IReadOnlyList<OfferTravellerDto> Travellers,
        IReadOnlyList<BackofficeSeatOfferDto> Offers,
        IReadOnlyList<BackofficeSeatOfferDto> BlockedOffers,
        IReadOnlyList<OfferDiagnosticDto> Diagnostics,
        OfferCompletenessDto Completeness);

    public sealed record BackofficeSeatOfferDto(
        string OfferItemRef,
        long PointOfSaleId,
        OfferCandidateIdentityDto CandidateIdentity,
        OfferServiceDto Service,
        OfferCoverageDto Coverage,
        OfferEligibilityDto Eligibility,
        CustomerSelectionContractDto Selection,
        OfferQuantityDto Quantity,
        OfferPricingDto Pricing,
        BackofficeSeatTermsDto Seat,
        OfferConfirmationDto Confirmation,
        OfferDocumentDto Document,
        EnumValueDto OfferReadiness,
        EnumValueDto DisplayState,
        bool CanPurchase,
        IReadOnlyList<string> ReasonCodes);

    public sealed record BackofficeSeatTermsDto(
        EnumValueDto Purpose,
        IReadOnlyList<string> CharacteristicCodes,
        IReadOnlyList<int> CabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        EnumValueDto? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        IReadOnlyList<string> SeatNumbers);

    public sealed record BackofficeSeatOfferDetailDto(
        string OfferId,
        BackofficeSeatOfferDto Offer,
        SeatSpecificationDto Specification,
        BackofficeSeatRuleDto? SeatRule,
        BackofficeRestrictionsDto Restrictions,
        BackofficeTermsDto Terms,
        IReadOnlyList<BackofficeRateDto> PublishedRates);

    public sealed record BackofficeSeatSelectionResultDto(
        string OfferId,
        EnumValueDto Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool ProvisionChangedBySelection,
        BackofficeSeatOfferDto Offer,
        decimal? SelectedQuantityTotal,
        IReadOnlyList<BackofficeFieldIssueDto> FieldIssues,
        bool CanPurchase);
}
