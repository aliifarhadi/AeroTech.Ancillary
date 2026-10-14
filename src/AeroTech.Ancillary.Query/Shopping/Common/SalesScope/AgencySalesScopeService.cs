using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed class AgencySalesScopeService : IAgencySalesScopeService
    {
        private readonly ICallerContext _caller;
        private readonly IRequestCountry _country;
        private readonly IOperatorAirlineResolver _operatorAirline;
        private readonly IPointOfSaleEligibilityReference _pointsOfSale;

        public AgencySalesScopeService(
            ICallerContext caller,
            IRequestCountry country,
            IOperatorAirlineResolver operatorAirline,
            IPointOfSaleEligibilityReference pointsOfSale)
        {
            _caller = caller;
            _country = country;
            _operatorAirline = operatorAirline;
            _pointsOfSale = pointsOfSale;
        }

        public async Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default)
        {
            if (_caller.TravelAgencyId is null)
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            if (homeAirlineId is not (> 0 and <= int.MaxValue))
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            return new AncillarySalesScope(
                (int)homeAirlineId.Value,
                AgencySalesPolicy.SalesChannel,
                await AuthorizedPointsOfSale.ResolveAsync(
                    _pointsOfSale,
                    new PointOfSaleEligibilityQuery(AgencySalesPolicy.SalesChannel, _country.CountryId, [], _caller.TravelAgencyOfficeIds.FirstOrDefault()),
                    cancellationToken));
        }
    }
}
