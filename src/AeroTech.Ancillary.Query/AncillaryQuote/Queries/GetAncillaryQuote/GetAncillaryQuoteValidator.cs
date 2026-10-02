using FluentValidation;

namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote
{
    public abstract class GetAncillaryQuoteValidator<TQuery> : AbstractValidator<TQuery>
        where TQuery : IGetAncillaryQuoteQuery
    {
        private const int RefMaxLength = 50;

        protected GetAncillaryQuoteValidator()
        {
            RuleFor(query => query.CurrencyId).GreaterThan(0);
            RuleFor(query => query.AsOf).NotEmpty();

            RuleFor(query => query.Travellers).NotEmpty();
            RuleForEach(query => query.Travellers).NotNull().ChildRules(traveller =>
            {
                traveller.RuleFor(value => value.Ref).NotEmpty().MaximumLength(RefMaxLength);
                traveller.RuleFor(value => value.PassengerTypeCode).NotEmpty();
                traveller.RuleFor(value => value.FlightRefs).NotEmpty();
                traveller.RuleForEach(value => value.FlightRefs).NotEmpty();
            });

            RuleFor(query => query.Bounds).NotEmpty();
            RuleForEach(query => query.Bounds).NotNull().ChildRules(bound =>
            {
                bound.RuleFor(value => value.Ref).NotEmpty().MaximumLength(RefMaxLength);
                bound.RuleFor(value => value.Flights).NotEmpty();
                bound.RuleForEach(value => value.Flights).NotNull().ChildRules(flight =>
                {
                    flight.RuleFor(value => value.Ref).NotEmpty().MaximumLength(RefMaxLength);
                    flight.RuleFor(value => value.OriginAirportId).GreaterThan(0);
                    flight.RuleFor(value => value.DestinationAirportId).GreaterThan(0);
                    flight.RuleFor(value => value.DepartureDateTime).NotEmpty();
                    flight.RuleFor(value => value.MarketingAirlineId).GreaterThan(0);
                    flight.RuleFor(value => value.OperatingAirlineId).GreaterThan(0);
                });
            });

            RuleForEach(query => query.Selections).NotNull().ChildRules(selection =>
            {
                selection.RuleFor(value => value.ProductRef).NotEmpty().Matches("^[A-Z0-9]{2,20}$");
                selection.RuleFor(value => value.ProductVersion).GreaterThan(0);
                selection.RuleFor(value => value.TravellerRef).NotEmpty();
                selection.RuleFor(value => value.Quantity).GreaterThan(0);
            });

            RuleForEach(query => query.Existing).NotNull().ChildRules(existing =>
            {
                existing.RuleFor(value => value.ProductRef).NotEmpty();
                existing.RuleFor(value => value.TravellerRef).NotEmpty();
                existing.RuleFor(value => value.Quantity).GreaterThan(0);
            });
        }
    }
}
