using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public class AgencyApiReferencesTests
{
    private static readonly DateTimeOffset Departure = new(2027, 3, 1, 8, 0, 0, TimeSpan.Zero);

    private static FlightOfferLeg Leg(int sequence, int origin, int destination)
        => new(sequence, 9000 + sequence, origin, destination, Departure.AddHours(sequence), Departure.AddHours(sequence + 1));

    private static FlightOfferFlight Flight(int sequence, string number, int marketing, int operating, params FlightOfferLeg[] legs)
        => new(
            sequence,
            8000 + sequence,
            7000 + sequence,
            1,
            number,
            legs[0].OriginAirportId,
            null,
            legs[^1].DestinationAirportId,
            null,
            operating,
            marketing,
            legs[0].DepartureAt,
            legs[^1].ArrivalAt,
            null,
            1,
            null,
            "Y",
            legs);

    private static ApiOfferReferences References()
    {
        var offer = new FlightOffer(
            "offer",
            Departure,
            null,
            null,
            70,
            "IRR",
            0m,
            0m,
            0m,
            [
                new FlightOfferBound("B2", 2, null, 30, 10, [Flight(1, "114", 1, 1, Leg(1, 30, 10))]),
                new FlightOfferBound(
                    "B1",
                    1,
                    null,
                    10,
                    30,
                    [Flight(2, "2022", 1, 2, Leg(1, 20, 30)), Flight(1, "111", 1, 1, Leg(2, 15, 20), Leg(1, 10, 15))])
            ],
            [],
            [
                new FlightOfferTicket("INF-1", 3, PassengerTypeCode.INF, []),
                new FlightOfferTicket("ADT-1", 1, PassengerTypeCode.ADT, []),
                new FlightOfferTicket("CHD-1", 2, PassengerTypeCode.CHD, [])
            ],
            [],
            [],
            Channel.PartnerAPI,
            null);

        return new ApiOfferReferences(
            offer,
            new Dictionary<int, string> { [10] = "THR", [15] = "MHD", [20] = "IST", [30] = "FRA" },
            new Dictionary<int, string> { [1] = "W5", [2] = "TK" },
            new Dictionary<int, string> { [70] = "IRR" },
            new Dictionary<long, BaggageSpecificationArgs>());
    }

    [Fact]
    public void P32C_API_01_segments_are_numbered_per_leg_across_bounds_and_travellers_by_their_index()
    {
        var references = References();

        Assert.Equal(new[] { "1", "2", "3", "4" }, references.Segments.Select(segment => segment.Id));
        Assert.Equal(
            new[] { "THR-MHD", "MHD-IST", "IST-FRA", "FRA-THR" },
            references.Segments.Select(segment => $"{segment.Departure.IataCode}-{segment.Arrival.IataCode}"));
        Assert.Equal(new[] { "111", "111", "2022", "114" }, references.Segments.Select(segment => segment.Number));
        Assert.Equal(new[] { "W5", "W5", "W5", "W5" }, references.Segments.Select(segment => segment.CarrierCode));
        Assert.Equal(new[] { "W5", "W5", "TK", "W5" }, references.Segments.Select(segment => segment.Operating.CarrierCode));
        Assert.Equal(new[] { "1", "2" }, references.SegmentIds(["B1:F1"]));
        Assert.Equal(new[] { "3" }, references.SegmentIds(["B1:F2"]));
        Assert.Equal(new[] { "4" }, references.SegmentIds(["B2:F1"]));
        Assert.Equal(new[] { "1", "2", "3", "4" }, references.SegmentIds(["B1:F1", "B1:F2", "B2:F1"]));
        Assert.Equal(new[] { "1|ADULT", "2|CHILD", "3|INFANT" }, references.Travelers.Select(traveler => $"{traveler.Id}|{traveler.TravelerType}"));
        Assert.Equal(new[] { "2", "1", "3" }, references.TravelerIds(["CHD-1", "ADT-1", "INF-1"]));
    }

    [Theory]
    [InlineData("5450000", "5450000.00")]
    [InlineData("12.5", "12.50")]
    [InlineData("1000.10", "1000.10")]
    [InlineData("0.123456", "0.123456")]
    [InlineData("0", "0.00")]
    public void P32C_API_02_an_amount_is_written_as_text_with_at_least_two_decimals_and_no_loss(string amount, string expected)
        => Assert.Equal(expected, ApiOfferReferences.Amount(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));

    [Fact]
    public void P32C_API_03_a_code_that_the_reference_data_does_not_hold_is_left_empty_and_never_guessed()
    {
        var references = References();

        Assert.Null(references.CurrencyCode(155));
        Assert.Null(references.CurrencyCode(null));
        Assert.Equal("IRR", references.CurrencyCode(70));
    }
}
