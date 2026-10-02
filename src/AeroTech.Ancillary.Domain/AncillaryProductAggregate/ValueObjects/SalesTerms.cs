using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects
{
    public sealed class SalesTerms : ValueObject
    {
        private const int FormOfRefundCodeMaxLength = 10;

        private SalesTerms()
        {
        }

        public SalesTerms(
            bool refundable,
            bool? commissionable,
            bool? reusable,
            string? formOfRefundCode,
            bool? interlineSettlementAllowed)
        {
            if (formOfRefundCode is { Length: > FormOfRefundCodeMaxLength })
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(SalesTerms)}.{nameof(FormOfRefundCode)}");

            Refundable = refundable;
            Commissionable = commissionable;
            Reusable = reusable;
            FormOfRefundCode = formOfRefundCode;
            InterlineSettlementAllowed = interlineSettlementAllowed;
        }

        public bool Refundable { get; private set; }

        public bool? Commissionable { get; private set; }

        public bool? Reusable { get; private set; }

        public string? FormOfRefundCode { get; private set; }

        public bool? InterlineSettlementAllowed { get; private set; }

        public SalesTerms Copy() => new(Refundable, Commissionable, Reusable, FormOfRefundCode, InterlineSettlementAllowed);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Refundable;
            yield return Commissionable;
            yield return Reusable;
            yield return FormOfRefundCode;
            yield return InterlineSettlementAllowed;
        }
    }
}
