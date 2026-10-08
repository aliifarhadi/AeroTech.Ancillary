using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class ProvisionApplication : ValueObject
    {
        private ProvisionApplication()
        {
        }

        private ProvisionApplication(ProvisionApplicationType type, BaggageApplication? baggage)
        {
            Type = type;
            Baggage = baggage;
        }

        public ProvisionApplicationType Type { get; private set; }

        public BaggageApplication? Baggage { get; private set; }

        public static ProvisionApplication Create(ProvisionApplicationType type, BaggageApplication? baggage)
        {
            Require(Enum.IsDefined(type), nameof(Type));
            Require(type == ProvisionApplicationType.Baggage ? baggage is not null : baggage is null, nameof(Baggage));

            return new ProvisionApplication(type, baggage);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Type;
            yield return Baggage;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionApplication)}.{field}");
        }
    }
}
