using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.ServiceHost.CallerContext
{
    public sealed class HeaderRequestCountry : IRequestCountry
    {
        public const string CountryHeader = "X-Country";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderRequestCountry(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public long? CountryId
            => long.TryParse(_httpContextAccessor.HttpContext?.Request.Headers[CountryHeader].ToString(), out var countryId) ? countryId : null;
    }
}
