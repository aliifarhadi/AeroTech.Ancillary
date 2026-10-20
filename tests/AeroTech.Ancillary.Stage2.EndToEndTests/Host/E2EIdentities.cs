using AeroTech.Ancillary.ServiceHost.CallerContext;
using AeroTech.Messages.Shared;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public static class E2EIdentities
{
    public const string Header = "X-E2E-Identity";
    public const string Backoffice = "backoffice";
    public const string Api = "api";
    public const string OtaPanel = "otapanel";
    public const string ForeignApi = "api-foreign-agency";
    public const long AirlineOfficeId = 9200;
    public const long TravelAgencyId = 31;
    public const long TravelAgencyOfficeId = 33;
    public const long CustomerId = 9000000000000000900;
    public const long ForeignCustomerId = 9000000000000000901;

    private const string SurfaceClaim = "authz_surface";
    private const string ScopeClaim = "scope";

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<(string Type, string Value)>> Claims =
        new Dictionary<string, IReadOnlyList<(string Type, string Value)>>(StringComparer.Ordinal)
        {
            [Backoffice] =
            [
                (SurfaceClaim, "backoffice"),
                (ScopeClaim, AuthorizationSurfaceScopes.Backoffice),
                (ClaimsCallerContext.ContextTypeClaim, "Airline"),
                (ClaimsCallerContext.PrincipalTypeClaim, "human"),
                (ClaimsCallerContext.AirlineUserIdClaim, "501"),
                (ClaimsCallerContext.AirlineOfficeIdClaim, AirlineOfficeId.ToString())
            ],
            [Api] = Agency("api", AuthorizationSurfaceScopes.Api, "PartnerApi", "travel_agency_api", CustomerId, TravelAgencyId),
            [OtaPanel] = Agency("otapanel", AuthorizationSurfaceScopes.OtaPanel, "TravelAgency", "human", CustomerId, TravelAgencyId),
            [ForeignApi] = Agency("api", AuthorizationSurfaceScopes.Api, "PartnerApi", "travel_agency_api", ForeignCustomerId, TravelAgencyId + 1)
        };

    private static IReadOnlyList<(string Type, string Value)> Agency(string surface, string scope, string contextType, string principalType, long customerId, long agencyId)
        =>
        [
            (SurfaceClaim, surface),
            (ScopeClaim, scope),
            (ClaimsCallerContext.ContextTypeClaim, contextType),
            (ClaimsCallerContext.PrincipalTypeClaim, principalType),
            (ClaimsCallerContext.TravelAgencyUserIdClaim, "41"),
            (ClaimsCallerContext.TravelAgencyIdClaim, agencyId.ToString()),
            (ClaimsCallerContext.TravelAgencyOfficeIdClaim, TravelAgencyOfficeId.ToString()),
            (ClaimsCallerContext.PartnerApiAccessProfileIdClaim, "35"),
            (ClaimsCallerContext.CustomerIdClaim, customerId.ToString())
        ];
}
