using System.Globalization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice
{
    public static class BackofficeSelectionBatchMapper
    {
        public static BackofficeSelectionBatchDto Map(SelectionBatchSnapshot batch)
            => new(batch.Shopping.Offer.OfferId, batch.Lines.Select(Line).ToList(), OfferSelectionBatchMapper.Basket(batch.Basket));

        private static BackofficeSelectionLineDto Line(SelectionLineSnapshot line)
            => new(
                line.ClientSelectionId,
                EnumValueDto.Of(line.Kind),
                EnumValueDto.Of(line.Status),
                line.ReasonCodes,
                line.CanPurchase,
                line.PricedSelection?.Value,
                line.PricedSelection?.ExpiresAtUtc,
                line.Ancillary is { } ancillary ? BackofficeSelectionResultMapper.Map(ancillary, line.CanPurchase, line.PricedSelection) : null,
                line.Seat is null
                    ? null
                    : OfferSelectionBatchMapper.Seat(line, travellerRef => travellerRef, flight => flight.Flight.FlightId.ToString(CultureInfo.InvariantCulture)));
    }
}
