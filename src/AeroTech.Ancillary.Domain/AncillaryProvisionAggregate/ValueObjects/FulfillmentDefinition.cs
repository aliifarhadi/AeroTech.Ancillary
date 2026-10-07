using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class FulfillmentDefinition : ValueObject
    {
        private const int FulfillmentProviderKeyMaxLength = 50;

        private FulfillmentDefinition()
        {
        }

        private FulfillmentDefinition(string fulfillmentProviderKey) => FulfillmentProviderKey = fulfillmentProviderKey;

        public string FulfillmentProviderKey { get; private set; } = default!;

        public static FulfillmentDefinition Create(string fulfillmentProviderKey)
        {
            if (fulfillmentProviderKey is not { Length: >= 1 and <= FulfillmentProviderKeyMaxLength }
                || fulfillmentProviderKey.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(FulfillmentDefinition)}.{nameof(FulfillmentProviderKey)}");

            return new FulfillmentDefinition(fulfillmentProviderKey);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FulfillmentProviderKey;
        }
    }
}
