using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Snapshots
{
    public static class OfferItemRef
    {
        private const char Separator = '\n';

        public static string Of(string offerId, AncillarySalesScope scope, long pointOfSaleId, CandidateIdentity candidate)
            => Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
                Separator,
                scope.Channel.ToString(),
                scope.OwnerAirlineId.ToString(CultureInfo.InvariantCulture),
                pointOfSaleId.ToString(CultureInfo.InvariantCulture),
                offerId,
                candidate.DefinitionVersionId.ToString(CultureInfo.InvariantCulture),
                candidate.ProvisionId.ToString(CultureInfo.InvariantCulture),
                candidate.PricingRevisionId?.ToString(CultureInfo.InvariantCulture),
                candidate.CoverageScope.ToString(),
                candidate.TravellerRef,
                candidate.CoverageRef))));
    }
}
