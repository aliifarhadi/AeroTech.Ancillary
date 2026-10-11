using System.Globalization;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.FlightOffers;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public sealed class BackofficeSalesScopeResolver : IBackofficeSalesScopeResolver
    {
        private readonly ICallerContext _caller;
        private readonly IOperatorAirlineResolver _operatorAirline;
        private readonly IPointOfSaleEligibilityReference _pointsOfSale;
        private readonly IClock _clock;

        public BackofficeSalesScopeResolver(
            ICallerContext caller,
            IOperatorAirlineResolver operatorAirline,
            IPointOfSaleEligibilityReference pointsOfSale,
            IClock clock)
        {
            _caller = caller;
            _operatorAirline = operatorAirline;
            _pointsOfSale = pointsOfSale;
            _clock = clock;
        }

        public async Task<FlightOfferSalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default)
        {
            if (_caller is not { ContextType: CallerContextType.Airline, AirlineUserId: > 0, AirlineOfficeId: > 0 })
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            if (homeAirlineId is not (> 0 and <= int.MaxValue))
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            return new FlightOfferSalesScope(
                (int)homeAirlineId.Value,
                options?.PointOfSaleId ?? await EligiblePointOfSaleIdAsync(options?.CountryId, _caller.AirlineOfficeId.Value, cancellationToken),
                long.TryParse(options?.CustomerId, NumberStyles.None, CultureInfo.InvariantCulture, out var customerId) ? customerId : null,
                options?.PreferredCurrencyId,
                (options?.SalesDate ?? _clock.GetDateTime()).ToUniversalTime());
        }

        private async Task<long> EligiblePointOfSaleIdAsync(long? countryId, long airlineOfficeId, CancellationToken cancellationToken)
        {
            var eligibility = await _pointsOfSale.ResolveAsync(
                new PointOfSaleEligibilityQuery(Channel.BackOffice, countryId, [airlineOfficeId], null),
                cancellationToken);

            if (eligibility.Result != InventoryReferenceCheck.Verified)
                throw ExceptionFactory.PointOfSaleContextUnresolved("the point-of-sale reference is not connected and options.pointOfSaleId was not given");

            return eligibility.AllowedPointOfSaleIds.Count == 1
                ? eligibility.AllowedPointOfSaleIds.Single()
                : throw ExceptionFactory.PointOfSaleContextUnresolved(
                    $"{eligibility.AllowedPointOfSaleIds.Count} points of sale are allowed for this office and options.pointOfSaleId was not given");
        }
    }
}
