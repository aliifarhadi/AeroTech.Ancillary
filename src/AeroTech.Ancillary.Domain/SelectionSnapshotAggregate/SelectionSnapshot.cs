using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.SelectionSnapshotAggregate
{
    public sealed class SelectionSnapshot
    {
        private const string IdPrefix = "s_";
        private const int IdLength = 24;
        private const short CurrentVersion = 1;
        private const int OfferIdMaxLength = 512;

        private readonly List<SelectionSnapshotLine> _lines = new();

        private SelectionSnapshot()
        {
        }

        private SelectionSnapshot(
            string id,
            string? offerId,
            long? orderId,
            int? orderCommercialVersion,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            Id = id;
            Version = CurrentVersion;
            OfferId = offerId;
            OrderId = orderId;
            OrderCommercialVersion = orderCommercialVersion;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public string Id { get; private set; } = default!;

        public short Version { get; private set; }

        public string? OfferId { get; private set; }

        public long? OrderId { get; private set; }

        public int? OrderCommercialVersion { get; private set; }

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public DateTimeOffset ExpiresAtUtc { get; private set; }

        public IReadOnlyCollection<SelectionSnapshotLine> Lines => _lines.AsReadOnly();

        public ShoppingSourceKind SourceKind => OrderId is null ? ShoppingSourceKind.Offer : ShoppingSourceKind.Order;

        public static SelectionSnapshot CreateForOffer(
            string id,
            string offerId,
            IReadOnlyList<SelectionSnapshotLineArgs> lines,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            RequireId(id);
            Require(offerId is { Length: >= 1 and <= OfferIdMaxLength }, nameof(OfferId));

            return WithLines(new SelectionSnapshot(id, offerId, null, null, createdAtUtc, expiresAtUtc), lines);
        }

        public static SelectionSnapshot CreateForOrder(
            string id,
            long orderId,
            int orderCommercialVersion,
            IReadOnlyList<SelectionSnapshotLineArgs> lines,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            RequireId(id);
            Require(orderId > 0, nameof(OrderId));
            Require(orderCommercialVersion > 0, nameof(OrderCommercialVersion));

            return WithLines(new SelectionSnapshot(id, null, orderId, orderCommercialVersion, createdAtUtc, expiresAtUtc), lines);
        }

        public bool HasExpired(DateTimeOffset now) => now >= ExpiresAtUtc;

        private static SelectionSnapshot WithLines(SelectionSnapshot snapshot, IReadOnlyList<SelectionSnapshotLineArgs> lines)
        {
            Require(lines is { Count: > 0 }, nameof(Lines));
            Require(lines.Select(line => line.OfferItemId).Distinct(StringComparer.Ordinal).Count() == lines.Count, nameof(Lines));
            Require(snapshot.ExpiresAtUtc > snapshot.CreatedAtUtc, nameof(ExpiresAtUtc));

            snapshot._lines.AddRange(lines.Select((line, index) => SelectionSnapshotLine.Create(snapshot.Id, index + 1, line)));

            return snapshot;
        }

        private static void RequireId(string id)
            => Require(id is { Length: IdLength } && id.StartsWith(IdPrefix, StringComparison.Ordinal), nameof(Id));

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.SelectionSnapshotIsInvalid(field);
        }
    }
}
