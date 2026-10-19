using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferBaggage(int Pieces, decimal Weight, WeightUnit Unit);
}
