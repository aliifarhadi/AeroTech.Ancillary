using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;

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

        private SelectionSnapshot(string id, string offerId, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
        {
            Id = id;
            Version = CurrentVersion;
            OfferId = offerId;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public string Id { get; private set; } = default!;

        public short Version { get; private set; }

        public string OfferId { get; private set; } = default!;

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public DateTimeOffset ExpiresAtUtc { get; private set; }

        public IReadOnlyCollection<SelectionSnapshotLine> Lines => _lines.AsReadOnly();

        public static SelectionSnapshot Create(
            string id,
            string offerId,
            IReadOnlyList<SelectionSnapshotLineArgs> lines,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            Require(id is { Length: IdLength } && id.StartsWith(IdPrefix, StringComparison.Ordinal), nameof(Id));
            Require(offerId is { Length: >= 1 and <= OfferIdMaxLength }, nameof(OfferId));
            Require(lines is { Count: > 0 }, nameof(Lines));
            Require(lines.Select(line => line.OfferItemId).Distinct(StringComparer.Ordinal).Count() == lines.Count, nameof(Lines));
            Require(expiresAtUtc > createdAtUtc, nameof(ExpiresAtUtc));

            var snapshot = new SelectionSnapshot(id, offerId, createdAtUtc, expiresAtUtc);

            snapshot._lines.AddRange(lines.Select((line, index) => SelectionSnapshotLine.Create(id, index + 1, line)));

            return snapshot;
        }

        public bool HasExpired(DateTimeOffset now) => now >= ExpiresAtUtc;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.SelectionSnapshotIsInvalid(field);
        }
    }
}
