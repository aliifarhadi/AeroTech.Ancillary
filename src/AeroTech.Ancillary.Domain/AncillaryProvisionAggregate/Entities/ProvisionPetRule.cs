using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionPetRule : Entity<long>
    {
        private ProvisionPetRule()
        {
        }

        private ProvisionPetRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public string? CountryExceptionCode { get; private set; }

        public int? MinAnimalAgeWeeksOverride { get; private set; }

        public decimal? MaxCombinedKgOverride { get; private set; }

        public ConfirmationRequirement AcceptanceMode { get; private set; }

        internal static Func<ProvisionPetRule?> Plan(
            ProvisionPetRule? stored,
            long ancillaryProvisionId,
            ProvisionPetRuleArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null)
                return () => null;

            Require(args.CountryExceptionCode is null || (args.CountryExceptionCode.Length is >= 1 and <= 10 && args.CountryExceptionCode.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')), nameof(CountryExceptionCode));
            Require(args.MinAnimalAgeWeeksOverride is null or >= 0, nameof(MinAnimalAgeWeeksOverride));
            Require(args.MaxCombinedKgOverride is null || (args.MaxCombinedKgOverride > 0 && args.MaxCombinedKgOverride == decimal.Round(args.MaxCombinedKgOverride.Value, 3)), nameof(MaxCombinedKgOverride));
            Require(Enum.IsDefined(args.AcceptanceMode), nameof(AcceptanceMode));

            var rule = stored ?? new ProvisionPetRule(idGenerator.NewId(), ancillaryProvisionId);

            return () =>
            {
                rule.CountryExceptionCode = args.CountryExceptionCode;
                rule.MinAnimalAgeWeeksOverride = args.MinAnimalAgeWeeksOverride;
                rule.MaxCombinedKgOverride = args.MaxCombinedKgOverride;
                rule.AcceptanceMode = args.AcceptanceMode;

                return rule;
            };
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPetRule)}.{field}");
        }
    }
}
