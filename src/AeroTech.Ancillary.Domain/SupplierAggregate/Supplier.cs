using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.SupplierAggregate
{
    public sealed class Supplier : AggregateRoot<long>
    {
        private const int NameMaxLength = 100;
        private const int FulfillmentProviderKeyMaxLength = 50;

        private Supplier()
        {
        }

        private Supplier(
            long id,
            int ownerAirlineId,
            string name,
            SupplierFulfillmentKind fulfillmentKind,
            string? fulfillmentProviderKey,
            DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            Name = name;
            FulfillmentKind = fulfillmentKind;
            FulfillmentProviderKey = fulfillmentProviderKey;
            Status = SupplierStatus.Active;
            CreatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public string Name { get; private set; } = default!;

        public SupplierFulfillmentKind FulfillmentKind { get; private set; }

        public string? FulfillmentProviderKey { get; private set; }

        public SupplierStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public static Supplier Register(
            long id,
            int ownerAirlineId,
            string name,
            SupplierFulfillmentKind fulfillmentKind,
            string? fulfillmentProviderKey,
            DateTimeOffset createdAt)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(name is { Length: >= 1 and <= NameMaxLength }, nameof(Name));
            Require(Enum.IsDefined(fulfillmentKind), nameof(FulfillmentKind));
            Require(
                fulfillmentKind != SupplierFulfillmentKind.Local || fulfillmentProviderKey is null,
                nameof(FulfillmentProviderKey));
            Require(
                fulfillmentKind != SupplierFulfillmentKind.External || IsProviderKey(fulfillmentProviderKey),
                nameof(FulfillmentProviderKey));

            return new Supplier(id, ownerAirlineId, name, fulfillmentKind, fulfillmentProviderKey, createdAt);
        }

        public void Retire(DateTimeOffset now)
        {
            if (Status != SupplierStatus.Active)
                throw ExceptionFactory.SupplierStatusChangeNotAllowed();

            Status = SupplierStatus.Retired;
            RetiredAt = now;
        }

        private static bool IsProviderKey(string? value)
            => value is { Length: >= 1 and <= FulfillmentProviderKeyMaxLength } && !value.Any(char.IsWhiteSpace);

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.SupplierIsInvalid(field);
        }
    }
}
