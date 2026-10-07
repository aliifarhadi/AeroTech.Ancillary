using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class SettlementDefinition : ValueObject
    {
        private SettlementDefinition()
        {
        }

        private SettlementDefinition(
            ReissueRefundPolicy reissueRefund,
            FormOfRefund? formOfRefund,
            bool commissionable,
            bool interlineSettlement)
        {
            ReissueRefund = reissueRefund;
            FormOfRefund = formOfRefund;
            Commissionable = commissionable;
            InterlineSettlement = interlineSettlement;
        }

        public ReissueRefundPolicy ReissueRefund { get; private set; }

        public FormOfRefund? FormOfRefund { get; private set; }

        public bool Commissionable { get; private set; }

        public bool InterlineSettlement { get; private set; }

        public static SettlementDefinition Create(
            ReissueRefundPolicy reissueRefund,
            FormOfRefund? formOfRefund,
            bool commissionable,
            bool interlineSettlement)
        {
            Require(Enum.IsDefined(reissueRefund), nameof(ReissueRefund));
            Require(formOfRefund is null || Enum.IsDefined(formOfRefund.Value), nameof(FormOfRefund));

            return new SettlementDefinition(reissueRefund, formOfRefund, commissionable, interlineSettlement);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return ReissueRefund;
            yield return FormOfRefund;
            yield return Commissionable;
            yield return InterlineSettlement;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(SettlementDefinition)}.{field}");
        }
    }
}
