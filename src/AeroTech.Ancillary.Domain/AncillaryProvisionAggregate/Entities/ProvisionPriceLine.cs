using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionPriceLine : Entity<long>
    {
        private const int CodeMaxLength = 10;
        private const int NameMaxLength = 100;

        private ProvisionPriceLine()
        {
        }

        internal ProvisionPriceLine(long id, long ancillaryProvisionId, ProvisionPriceLineArgs args)
        {
            Require(Enum.IsDefined(args.Category), nameof(Category));
            Require(args.Code is null or { Length: >= 1 and <= CodeMaxLength }, nameof(Code));
            Require(args.Name is null or { Length: >= 1 and <= NameMaxLength }, nameof(Name));
            Require(args.UnitAmount > 0, nameof(UnitAmount));
            Require(args.UnitAmount == decimal.Round(args.UnitAmount, 2), nameof(UnitAmount));

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Category = args.Category;
            Code = args.Code;
            Name = args.Name;
            UnitAmount = args.UnitAmount;
        }

        public long AncillaryProvisionId { get; private set; }

        public AncillaryPriceLineCategory Category { get; private set; }

        public string? Code { get; private set; }

        public string? Name { get; private set; }

        public decimal UnitAmount { get; private set; }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPriceLine)}.{field}");
        }
    }
}
