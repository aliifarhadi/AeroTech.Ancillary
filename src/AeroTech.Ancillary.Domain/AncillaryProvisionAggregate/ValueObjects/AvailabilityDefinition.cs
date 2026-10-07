using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class AvailabilityDefinition : ValueObject
    {
        private AvailabilityDefinition()
        {
        }

        private AvailabilityDefinition(bool mustCheckAvailability) => MustCheckAvailability = mustCheckAvailability;

        public bool MustCheckAvailability { get; private set; }

        public static AvailabilityDefinition Create(bool mustCheckAvailability) => new(mustCheckAvailability);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return MustCheckAvailability;
        }
    }
}
