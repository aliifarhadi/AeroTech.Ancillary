using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class CommercialOutcome : ValueObject
    {
        private CommercialOutcome()
        {
        }

        private CommercialOutcome(CommercialDisposition disposition, bool documentRequired, bool bookingRequired)
        {
            Disposition = disposition;
            DocumentRequired = documentRequired;
            BookingRequired = bookingRequired;
        }

        public CommercialDisposition Disposition { get; private set; }

        public bool DocumentRequired { get; private set; }

        public bool BookingRequired { get; private set; }

        public static CommercialOutcome Create(CommercialDisposition disposition, bool documentRequired, bool bookingRequired)
        {
            if (!Enum.IsDefined(disposition))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(CommercialOutcome)}.{nameof(Disposition)}");

            return new CommercialOutcome(disposition, documentRequired, bookingRequired);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Disposition;
            yield return DocumentRequired;
            yield return BookingRequired;
        }
    }
}
