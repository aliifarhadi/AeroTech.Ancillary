using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class UnaccompaniedMinorDetails
    {
        private const string Name = nameof(UnaccompaniedMinorDetails);
        private const int MaxAge = 18;

        private readonly List<SpecificationReference> _allowedTransitAirports = new();

        private UnaccompaniedMinorDetails()
        {
        }

        public int MinAgeYears { get; private set; }

        public int MaxAgeYearsExclusive { get; private set; }

        public bool GuardianContactRequired { get; private set; }

        public MinorConnectionPolicy ConnectionPolicy { get; private set; }

        public IReadOnlyCollection<SpecificationReference> AllowedTransitAirports => _allowedTransitAirports.AsReadOnly();

        internal static UnaccompaniedMinorDetails Create(UnaccompaniedMinorDetailsArgs args)
        {
            var airports = SpecificationRules.References(args.AllowedTransitAirportIds?.Select(id => (long)id), Name, nameof(AllowedTransitAirports));

            SpecificationRules.Require(args.MinAgeYears >= 0, Name, nameof(MinAgeYears));
            SpecificationRules.Require(args.MaxAgeYearsExclusive > args.MinAgeYears && args.MaxAgeYearsExclusive <= MaxAge, Name, nameof(MaxAgeYearsExclusive));
            SpecificationRules.Require(args.GuardianContactRequired, Name, nameof(GuardianContactRequired));
            SpecificationRules.Require(Enum.IsDefined(args.ConnectionPolicy), Name, nameof(ConnectionPolicy));
            SpecificationRules.Require(args.ConnectionPolicy == MinorConnectionPolicy.ApprovedConnections || airports.Count == 0, Name, nameof(AllowedTransitAirports));

            var details = new UnaccompaniedMinorDetails
            {
                MinAgeYears = args.MinAgeYears,
                MaxAgeYearsExclusive = args.MaxAgeYearsExclusive,
                GuardianContactRequired = args.GuardianContactRequired,
                ConnectionPolicy = args.ConnectionPolicy
            };

            details._allowedTransitAirports.AddRange(airports);

            return details;
        }

        public UnaccompaniedMinorDetailsArgs ToArgs()
            => new(MinAgeYears, MaxAgeYearsExclusive, GuardianContactRequired, ConnectionPolicy, _allowedTransitAirports.Select(row => (int)row.ReferenceId).ToList());
    }
}
