using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.SelectionSnapshotAggregate
{
    public sealed class SelectionSnapshot
    {
        private const string IdPrefix = "s_";
        private const int IdLength = 24;
        private const short CurrentVersion = 1;
        private const int RefMaxLength = 100;
        private const int BindingLength = 43;

        private SelectionSnapshot()
        {
        }

        private SelectionSnapshot(
            string id,
            long pricingRateId,
            ServiceCoverageScope coverageScope,
            string? travellerRef,
            string coverageRef,
            string offerBinding,
            string salesBinding,
            int quantity,
            string selectedJson,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            Id = id;
            Version = CurrentVersion;
            PricingRateId = pricingRateId;
            CoverageScope = coverageScope;
            TravellerRef = travellerRef;
            CoverageRef = coverageRef;
            OfferBinding = offerBinding;
            SalesBinding = salesBinding;
            Quantity = quantity;
            SelectedJson = selectedJson;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public string Id { get; private set; } = default!;

        public short Version { get; private set; }

        public long PricingRateId { get; private set; }

        public ServiceCoverageScope CoverageScope { get; private set; }

        public string? TravellerRef { get; private set; }

        public string CoverageRef { get; private set; } = default!;

        public string OfferBinding { get; private set; } = default!;

        public string SalesBinding { get; private set; } = default!;

        public int Quantity { get; private set; }

        public string SelectedJson { get; private set; } = default!;

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public DateTimeOffset ExpiresAtUtc { get; private set; }

        public static SelectionSnapshot Create(
            string id,
            long pricingRateId,
            ServiceCoverageScope coverageScope,
            string? travellerRef,
            string coverageRef,
            string offerBinding,
            string salesBinding,
            int quantity,
            string selectedJson,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            Require(id is { Length: IdLength } && id.StartsWith(IdPrefix, StringComparison.Ordinal), nameof(Id));
            Require(pricingRateId > 0, nameof(PricingRateId));
            Require(Enum.IsDefined(coverageScope), nameof(CoverageScope));
            Require(
                coverageScope == ServiceCoverageScope.Order ? travellerRef is null : travellerRef is { Length: >= 1 and <= RefMaxLength },
                nameof(TravellerRef));
            Require(coverageRef is { Length: >= 1 and <= RefMaxLength }, nameof(CoverageRef));
            Require(offerBinding is { Length: BindingLength }, nameof(OfferBinding));
            Require(salesBinding is { Length: BindingLength }, nameof(SalesBinding));
            Require(quantity >= 1, nameof(Quantity));
            Require(!string.IsNullOrWhiteSpace(selectedJson), nameof(SelectedJson));
            Require(expiresAtUtc > createdAtUtc, nameof(ExpiresAtUtc));

            return new SelectionSnapshot(
                id,
                pricingRateId,
                coverageScope,
                travellerRef,
                coverageRef,
                offerBinding,
                salesBinding,
                quantity,
                selectedJson,
                createdAtUtc,
                expiresAtUtc);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.SelectionSnapshotIsInvalid(field);
        }
    }
}
