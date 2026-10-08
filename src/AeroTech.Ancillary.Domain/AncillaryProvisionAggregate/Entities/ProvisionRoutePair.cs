using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionRoutePair : Entity<long>
    {
        private ProvisionRoutePair()
        {
        }

        internal ProvisionRoutePair(long id, long ancillaryProvisionId, long provisionGeographyRuleId, ProvisionRoutePairArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionGeographyRuleId = provisionGeographyRuleId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionGeographyRuleId { get; private set; }

        public int OriginAirportId { get; private set; }

        public int DestinationAirportId { get; private set; }

        public RoutePairDirection Direction { get; private set; }

        internal void Change(ProvisionRoutePairArgs args)
        {
            Require(args.OriginAirportId > 0, nameof(OriginAirportId));
            Require(args.DestinationAirportId > 0, nameof(DestinationAirportId));
            Require(args.OriginAirportId != args.DestinationAirportId, nameof(DestinationAirportId));
            Require(Enum.IsDefined(args.Direction), nameof(Direction));

            OriginAirportId = args.OriginAirportId;
            DestinationAirportId = args.DestinationAirportId;
            Direction = args.Direction;
        }

        internal bool SameAs(ProvisionRoutePair other)
            => OriginAirportId == other.OriginAirportId
               && DestinationAirportId == other.DestinationAirportId
               && Direction == other.Direction;

        internal bool Overlaps(ProvisionRoutePair other)
            => (OriginAirportId == other.OriginAirportId && DestinationAirportId == other.DestinationAirportId)
               || ((Direction == RoutePairDirection.BothDirections || other.Direction == RoutePairDirection.BothDirections)
                   && OriginAirportId == other.DestinationAirportId
                   && DestinationAirportId == other.OriginAirportId);

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionRoutePair)}.{field}");
        }
    }
}
