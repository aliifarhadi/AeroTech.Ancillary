using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionServiceLocation : Entity<long>
    {
        private ProvisionServiceLocation()
        {
        }

        internal ProvisionServiceLocation(long id, long ancillaryProvisionId, long provisionGeographyRuleId, ProvisionServiceLocationArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionGeographyRuleId = provisionGeographyRuleId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionGeographyRuleId { get; private set; }

        public ServiceLocationType LocationType { get; private set; }

        public int LocationId { get; private set; }

        internal void Change(ProvisionServiceLocationArgs args)
        {
            Require(Enum.IsDefined(args.LocationType), nameof(LocationType));
            Require(args.LocationId > 0, nameof(LocationId));

            LocationType = args.LocationType;
            LocationId = args.LocationId;
        }

        internal bool SameAs(ProvisionServiceLocation other)
            => LocationType == other.LocationType && LocationId == other.LocationId;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionServiceLocation)}.{field}");
        }
    }
}
