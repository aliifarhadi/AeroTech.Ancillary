using System.Globalization;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
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

        public async Task<AncillarySalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default)
        {
            if (_caller is not { ContextType: CallerContextType.Airline, AirlineUserId: > 0, AirlineOfficeId: > 0 })
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            if (homeAirlineId is not (> 0 and <= int.MaxValue))
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            return new AncillarySalesScope(
                (int)homeAirlineId.Value,
                Channel.BackOffice,
                options?.PointOfSaleId is { } named
                    ? [named]
                    : await AuthorizedPointsOfSale.ResolveAsync(
                        _pointsOfSale,
                        new PointOfSaleEligibilityQuery(Channel.BackOffice, options?.CountryId, [_caller.AirlineOfficeId.Value], null),
                        cancellationToken),
                long.TryParse(options?.CustomerId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var customerId) ? customerId : null,
                options?.PreferredCurrencyId is 0 ? null : options?.PreferredCurrencyId,
                (options?.SalesDate ?? _clock.GetDateTime()).ToUniversalTime());
        }
    }
}
