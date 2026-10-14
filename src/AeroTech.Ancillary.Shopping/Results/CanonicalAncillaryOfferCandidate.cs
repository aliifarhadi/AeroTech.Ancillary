using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record CanonicalAncillaryOfferCandidate
    {
        public required CandidateIdentity CandidateIdentity { get; init; }

        public required string ServiceDefinitionRef { get; init; }

        public required long DefinitionVersionId { get; init; }

        public required int DefinitionVersion { get; init; }

        public required long SupplierId { get; init; }

        public required long ProvisionId { get; init; }

        public required int ProvisionSequence { get; init; }

        public required AncillaryProfile Profile { get; init; }

        public required string VariantCode { get; init; }

        public required string ServiceTypeCode { get; init; }

        public required string ServiceSubCode { get; init; }

        public required string CommercialName { get; init; }

        public string? Description { get; init; }

        public required BookingSummary Booking { get; init; }

        public required DocumentRoutingSummary Document { get; init; }

        public required SelectionCoverage Scope { get; init; }

        public required EligibilityAssessment Eligibility { get; init; }

        public required CustomerSelectionContract Selection { get; init; }

        public required QuantityAssessment Quantity { get; init; }

        public required PriceAssessment Price { get; init; }

        public required AvailabilityAssessment Availability { get; init; }

        public required ConfirmationAssessment Confirmation { get; init; }

        public required OfferReadiness OfferReadiness { get; init; }

        public DateTimeOffset? ExpiresAtUtc { get; init; }

        public required IReadOnlyList<string> ReasonCodes { get; init; }

        public IReadOnlyList<string> Travellers => Scope.TravellerRefs;

        public IReadOnlyList<string> FlightRefs => Scope.FlightRefs;

        public IReadOnlyList<string> PortionRefs => Scope.PortionRefs;
    }

    public sealed record BookingSummary(BookingMethod Method, string? SsrCode, bool BookingRequired);

    public sealed record DocumentRoutingSummary(DocumentRouting Routing, AncillaryDocumentType DocumentType, string? Rfic, string? Rfisc, bool DocumentRequired);

    public sealed record SelectionCoverage(
        ServiceCoverageScope CoverageScope,
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<string> FlightRefs,
        IReadOnlyList<string> PortionRefs);

    public sealed record EligibilityAssessment(
        EligibilityStatus Status,
        long DefinitionVersionId,
        long ProvisionId,
        IReadOnlyList<string> ReasonCodes,
        IReadOnlyList<string> MissingContextFields);
}
