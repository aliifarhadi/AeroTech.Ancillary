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
        private const string SeatPrefix = "SEAT";

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

        public static string OfSeat(
            string offerId,
            AncillarySalesScope scope,
            long pointOfSaleId,
            string travellerRef,
            string flightRef,
            string seatNumber,
            CandidateIdentity? candidate)
            => Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
                Separator,
                SeatPrefix,
                scope.Channel.ToString(),
                scope.OwnerAirlineId.ToString(CultureInfo.InvariantCulture),
                pointOfSaleId.ToString(CultureInfo.InvariantCulture),
                offerId,
                travellerRef,
                flightRef,
                seatNumber.Trim().ToUpperInvariant(),
                candidate?.DefinitionVersionId.ToString(CultureInfo.InvariantCulture),
                candidate?.ProvisionId.ToString(CultureInfo.InvariantCulture),
                candidate?.PricingRevisionId?.ToString(CultureInfo.InvariantCulture)))));
    }
}
