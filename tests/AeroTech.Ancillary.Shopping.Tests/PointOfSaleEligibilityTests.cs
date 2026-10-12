using AeroTech.Ancillary.ReferenceData.PointOfSales;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public class PointOfSaleEligibilityTests
{
    private const long Pos = 1;

    private static long[] Allowed(
        PointOfSaleConditionReadModel[] conditions,
        long? country = null,
        long[]? airlineOffices = null,
        long? agencyOffice = null,
        string channel = "BackOffice",
        long[]? pointsOfSale = null)
        => PointOfSaleEligibilityResolver
            .ResolveAllowedPointOfSaleIds(pointsOfSale ?? [Pos], conditions, country, airlineOffices ?? [], agencyOffice, channel)
            .Order()
            .ToArray();

    private static PointOfSaleConditionReadModel Condition(long id = 1, long pointOfSale = Pos) => new() { Id = id, PointOfSaleId = pointOfSale };

    [Fact]
    public void POS_01_a_point_of_sale_without_conditions_is_never_allowed_and_one_matching_condition_is_enough()
    {
        var conditions = new[]
        {
            new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = 1, CountryIsInclude = true, CountryIds = [5] },
            new PointOfSaleConditionReadModel { Id = 2, PointOfSaleId = 1, ChannelIsInclude = true, Channels = ["API"] },
            new PointOfSaleConditionReadModel { Id = 3, PointOfSaleId = 3, CountryIsInclude = true, CountryIds = [9] }
        };

        Assert.Equal(new long[] { 1 }, Allowed(conditions, country: 5, pointsOfSale: [1, 2, 3]));
        Assert.Equal(new long[] { 1 }, Allowed(conditions, country: 7, channel: "API", pointsOfSale: [1, 2, 3]));
        Assert.Equal(new long[] { 3 }, Allowed(conditions, country: 9, pointsOfSale: [1, 2, 3]));
        Assert.Empty(Allowed(conditions, country: 7, pointsOfSale: [1, 2, 3]));
        Assert.Empty(Allowed([], pointsOfSale: [1, 2, 3]));
    }

    [Fact]
    public void POS_02_an_unconstrained_condition_allows_every_caller()
        => Assert.Equal(new long[] { Pos }, Allowed([Condition()], country: 5, airlineOffices: [10], agencyOffice: 7, channel: "GDS"));

    [Fact]
    public void POS_03_the_country_is_optional_on_the_request_and_follows_the_include_flag_when_given()
    {
        var include = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, CountryIsInclude = true, CountryIds = [5] } };
        var unflagged = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, CountryIds = [5] } };
        var exclude = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, CountryIsInclude = false, CountryIds = [116] } };

        Assert.NotEmpty(Allowed(include, country: 5));
        Assert.Empty(Allowed(include, country: 6));
        Assert.NotEmpty(Allowed(include));
        Assert.NotEmpty(Allowed(unflagged, country: 5));
        Assert.Empty(Allowed(unflagged, country: 6));
        Assert.Empty(Allowed(exclude, country: 116));
        Assert.NotEmpty(Allowed(exclude, country: 5));
        Assert.NotEmpty(Allowed(exclude));
    }

    [Fact]
    public void POS_04_any_airline_office_of_the_caller_satisfies_an_include_and_breaks_an_exclude()
    {
        var include = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, AirlineOfficeIsInclude = true, AirlineOfficeIds = [10] } };
        var exclude = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, AirlineOfficeIsInclude = false, AirlineOfficeIds = [10] } };

        Assert.NotEmpty(Allowed(include, airlineOffices: [11, 10]));
        Assert.Empty(Allowed(include, airlineOffices: [11]));
        Assert.Empty(Allowed(include));
        Assert.Empty(Allowed(exclude, airlineOffices: [11, 10]));
        Assert.NotEmpty(Allowed(exclude, airlineOffices: [11]));
        Assert.NotEmpty(Allowed(exclude));
    }

    [Fact]
    public void POS_05_the_travel_agency_office_and_the_channel_are_required_once_the_condition_names_them()
    {
        var office = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, TravelAgencyOfficeIsInclude = true, TravelAgencyOfficeIds = [7] } };
        var notOffice = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, TravelAgencyOfficeIsInclude = false, TravelAgencyOfficeIds = [7] } };
        var channel = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, ChannelIsInclude = true, Channels = ["IBE", "API"] } };
        var notChannel = new[] { new PointOfSaleConditionReadModel { Id = 1, PointOfSaleId = Pos, ChannelIsInclude = false, Channels = ["IBE", "API"] } };

        Assert.NotEmpty(Allowed(office, agencyOffice: 7));
        Assert.Empty(Allowed(office, agencyOffice: 8));
        Assert.Empty(Allowed(office));
        Assert.Empty(Allowed(notOffice, agencyOffice: 7));
        Assert.NotEmpty(Allowed(notOffice));
        Assert.NotEmpty(Allowed(channel, channel: "API"));
        Assert.Empty(Allowed(channel, channel: "BackOffice"));
        Assert.Empty(Allowed(notChannel, channel: "IBE"));
        Assert.NotEmpty(Allowed(notChannel, channel: "BackOffice"));
    }

    [Fact]
    public void POS_06_every_dimension_of_one_condition_must_match_together()
    {
        var conditions = new[]
        {
            new PointOfSaleConditionReadModel
            {
                Id = 1,
                PointOfSaleId = Pos,
                CountryIsInclude = true,
                CountryIds = [5],
                TravelAgencyOfficeIsInclude = true,
                TravelAgencyOfficeIds = [7],
                ChannelIsInclude = true,
                Channels = ["API"]
            }
        };

        Assert.NotEmpty(Allowed(conditions, country: 5, agencyOffice: 7, channel: "API"));
        Assert.Empty(Allowed(conditions, country: 6, agencyOffice: 7, channel: "API"));
        Assert.Empty(Allowed(conditions, country: 5, agencyOffice: 8, channel: "API"));
        Assert.Empty(Allowed(conditions, country: 5, agencyOffice: 7, channel: "GUI"));
    }
}
