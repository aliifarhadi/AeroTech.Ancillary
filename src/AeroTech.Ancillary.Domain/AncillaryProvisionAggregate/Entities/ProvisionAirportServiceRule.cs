using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAirportServiceRule : Entity<long>
    {
        private ProvisionAirportServiceRule()
        {
        }

        private ProvisionAirportServiceRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public string? TerminalRef { get; private set; }

        public AirportServiceDirection? Direction { get; private set; }

        public TimeOnly? ServiceWindowStart { get; private set; }

        public TimeOnly? ServiceWindowEnd { get; private set; }

        public long? FacilityId { get; private set; }

        public int? MaxGuestsPerPrimary { get; private set; }

        internal static Func<ProvisionAirportServiceRule?> Plan(
            ProvisionAirportServiceRule? stored,
            long ancillaryProvisionId,
            ProvisionAirportServiceRuleArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null)
                return () => null;

            Require(args.TerminalRef is null || (args.TerminalRef.Length is >= 1 and <= 30 && args.TerminalRef.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')), nameof(TerminalRef));
            Require(args.Direction is null || Enum.IsDefined(args.Direction.Value), nameof(Direction));
            Require(args.ServiceWindowStart is null == args.ServiceWindowEnd is null && (args.ServiceWindowStart is null || args.ServiceWindowStart != args.ServiceWindowEnd), nameof(ServiceWindowEnd));
            Require(args.FacilityId is null or > 0, nameof(FacilityId));
            Require(args.MaxGuestsPerPrimary is null or >= 0, nameof(MaxGuestsPerPrimary));
            Require(args.TerminalRef is not null || args.Direction is not null || args.ServiceWindowStart is not null || args.FacilityId is not null || args.MaxGuestsPerPrimary is not null, nameof(TerminalRef));

            var rule = stored ?? new ProvisionAirportServiceRule(idGenerator.NewId(), ancillaryProvisionId);

            return () =>
            {
                rule.TerminalRef = args.TerminalRef;
                rule.Direction = args.Direction;
                rule.ServiceWindowStart = args.ServiceWindowStart;
                rule.ServiceWindowEnd = args.ServiceWindowEnd;
                rule.FacilityId = args.FacilityId;
                rule.MaxGuestsPerPrimary = args.MaxGuestsPerPrimary;

                return rule;
            };
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAirportServiceRule)}.{field}");
        }
    }
}
