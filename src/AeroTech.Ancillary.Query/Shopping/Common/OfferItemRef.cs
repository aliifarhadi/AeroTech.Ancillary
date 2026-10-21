using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class OfferItemRef
    {
        private const char Separator = '\n';
        private const string OptionPrefix = "OPTION";

        public static string OptionOf(string offerItemRef, string fieldName, string value)
            => Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(Separator, OptionPrefix, offerItemRef, fieldName, value))));

        public static string Of(string offerId, string? sourceVersion, AncillarySalesScope scope, long pointOfSaleId, CandidateIdentity candidate)
            => Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
                Separator,
                scope.Channel.ToString(),
                scope.OwnerAirlineId.ToString(CultureInfo.InvariantCulture),
                scope.TravelAgencyId?.ToString(CultureInfo.InvariantCulture),
                scope.AirlineOfficeId?.ToString(CultureInfo.InvariantCulture),
                pointOfSaleId.ToString(CultureInfo.InvariantCulture),
                offerId,
                sourceVersion,
                candidate.DefinitionVersionId.ToString(CultureInfo.InvariantCulture),
                candidate.ProvisionId.ToString(CultureInfo.InvariantCulture),
                candidate.PricingRevisionId?.ToString(CultureInfo.InvariantCulture),
                candidate.CoverageScope.ToString(),
                candidate.TravellerRef,
                candidate.CoverageRef))));
    }
}
