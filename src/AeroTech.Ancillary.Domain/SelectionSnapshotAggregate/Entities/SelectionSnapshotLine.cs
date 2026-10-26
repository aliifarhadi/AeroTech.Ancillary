using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities
{
    public sealed class SelectionSnapshotLine
    {
        private const int OfferItemIdMaxLength = 1024;
        private const int RefMaxLength = 100;
        private const int BindingLength = 43;

        private SelectionSnapshotLine()
        {
        }

        private SelectionSnapshotLine(string selectionSnapshotId, int sequence, SelectionSnapshotLineArgs args)
        {
            SelectionSnapshotId = selectionSnapshotId;
            Sequence = sequence;
            OfferItemId = args.OfferItemId;
            PricingRateId = args.PricingRateId;
            CoverageScope = args.CoverageScope;
            TravellerRef = args.TravellerRef;
            CoverageRef = args.CoverageRef;
            OfferBinding = args.OfferBinding;
            SalesBinding = args.SalesBinding;
            Quantity = args.Quantity;
            SelectedJson = args.SelectedJson;
        }

        public string SelectionSnapshotId { get; private set; } = default!;

        public int Sequence { get; private set; }

        public string OfferItemId { get; private set; } = default!;

        public long PricingRateId { get; private set; }

        public ServiceCoverageScope CoverageScope { get; private set; }

        public string? TravellerRef { get; private set; }

        public string CoverageRef { get; private set; } = default!;

        public string OfferBinding { get; private set; } = default!;

        public string SalesBinding { get; private set; } = default!;

        public int Quantity { get; private set; }

        public string? SelectedJson { get; private set; }

        internal static SelectionSnapshotLine Create(string selectionSnapshotId, int sequence, SelectionSnapshotLineArgs args)
        {
            Require(args.OfferItemId is { Length: >= 1 and <= OfferItemIdMaxLength }, nameof(OfferItemId));
            Require(args.PricingRateId > 0, nameof(PricingRateId));
            Require(Enum.IsDefined(args.CoverageScope), nameof(CoverageScope));
            Require(
                args.CoverageScope == ServiceCoverageScope.Order ? args.TravellerRef is null : args.TravellerRef is { Length: >= 1 and <= RefMaxLength },
                nameof(TravellerRef));
            Require(args.CoverageRef is { Length: >= 1 and <= RefMaxLength }, nameof(CoverageRef));
            Require(args.OfferBinding is { Length: BindingLength }, nameof(OfferBinding));
            Require(args.SalesBinding is { Length: BindingLength }, nameof(SalesBinding));
            Require(args.Quantity >= 1, nameof(Quantity));
            Require(args.SelectedJson is null || !string.IsNullOrWhiteSpace(args.SelectedJson), nameof(SelectedJson));

            return new SelectionSnapshotLine(selectionSnapshotId, sequence, args);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.SelectionSnapshotIsInvalid($"{nameof(SelectionSnapshotLine)}.{field}");
        }
    }
}
