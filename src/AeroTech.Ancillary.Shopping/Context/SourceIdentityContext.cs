using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record SourceIdentityContext
    {
        public required ShoppingSourceKind SourceKind { get; init; }

        public string? TrustedSourceReference { get; init; }

        public string? TrustedSourceVersion { get; init; }

        public long? OrderId { get; init; }

        public string? OrderCommercialVersion { get; init; }

        public DateTimeOffset? TicketedAtUtc { get; init; }

        public DateTimeOffset? ValidUntilUtc { get; init; }
    }
}
