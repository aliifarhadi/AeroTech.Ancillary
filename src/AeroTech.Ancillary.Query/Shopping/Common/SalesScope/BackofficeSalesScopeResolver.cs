using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed class BackofficeSalesScopeResolver : IBackofficeSalesScopeResolver
    {
        private readonly ICallerContext _caller;
        private readonly IOperatorAirlineResolver _operatorAirline;
        private readonly IPointOfSaleEligibilityReference _pointsOfSale;

        public BackofficeSalesScopeResolver(
            ICallerContext caller,
            IOperatorAirlineResolver operatorAirline,
            IPointOfSaleEligibilityReference pointsOfSale)
        {
            _caller = caller;
            _operatorAirline = operatorAirline;
            _pointsOfSale = pointsOfSale;
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
                AirlineOfficeId: _caller.AirlineOfficeId.Value,
                CountryId: options?.CountryId);
        }
    }
}
