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

        internal ProvisionRoutePair(long id, long ancillaryProvisionId, ProvisionRoutePairArgs args)
        {
            Require(args.OriginAirportId > 0, nameof(OriginAirportId));
            Require(args.DestinationAirportId > 0, nameof(DestinationAirportId));
            Require(args.OriginAirportId != args.DestinationAirportId, nameof(DestinationAirportId));
            Require(Enum.IsDefined(args.Direction), nameof(Direction));

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            OriginAirportId = args.OriginAirportId;
            DestinationAirportId = args.DestinationAirportId;
            Direction = args.Direction;
        }

        public long AncillaryProvisionId { get; private set; }

        public int OriginAirportId { get; private set; }

        public int DestinationAirportId { get; private set; }

        public RoutePairDirection Direction { get; private set; }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionRoutePair)}.{field}");
        }
    }
}
