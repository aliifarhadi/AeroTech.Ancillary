using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Messages.Core.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public class PointOfSaleEligibilityTests
{
    private const long Pos = 1;

    private static async Task<long[]> AllowedAsync(
        PointOfSaleCondition[] conditions,
        long? country = null,
        long[]? airlineOffices = null,
        long? agencyOffice = null,
        Channel channel = Channel.BackOffice,
        long[]? pointsOfSale = null)
    {
        var service = new PointOfSaleEligibilityService(new FixedConditions(new PointOfSaleConditionSet(pointsOfSale ?? [Pos], conditions)));
        var eligibility = await service.ResolveAsync(new PointOfSaleEligibilityQuery(channel, country, airlineOffices ?? [], agencyOffice));

        Assert.Equal(InventoryReferenceCheck.Verified, eligibility.Result);

        return eligibility.AllowedPointOfSaleIds.Order().ToArray();
    }

    [Fact]
    public async Task POS_01_a_point_of_sale_without_conditions_is_never_allowed_and_one_matching_condition_is_enough()
    {
        var conditions = new[]
        {
            new PointOfSaleCondition { PointOfSaleId = 1, CountryIsInclude = true, CountryIds = [5] },
            new PointOfSaleCondition { PointOfSaleId = 1, ChannelIsInclude = true, Channels = ["API"] },
            new PointOfSaleCondition { PointOfSaleId = 3, CountryIsInclude = true, CountryIds = [9] }
        };

        Assert.Equal(new long[] { 1 }, await AllowedAsync(conditions, country: 5, pointsOfSale: [1, 2, 3]));
        Assert.Equal(new long[] { 1 }, await AllowedAsync(conditions, country: 7, channel: Channel.PartnerAPI, pointsOfSale: [1, 2, 3]));
        Assert.Equal(new long[] { 3 }, await AllowedAsync(conditions, country: 9, pointsOfSale: [1, 2, 3]));
        Assert.Empty(await AllowedAsync(conditions, country: 7, pointsOfSale: [1, 2, 3]));
        Assert.Empty(await AllowedAsync(conditions, country: 9, pointsOfSale: [1, 2]));
        Assert.Empty(await AllowedAsync([], pointsOfSale: [1, 2, 3]));
    }

    [Fact]
    public async Task POS_02_an_unconstrained_condition_allows_every_caller()
        => Assert.Equal(
            new long[] { Pos },
            await AllowedAsync([new PointOfSaleCondition { PointOfSaleId = Pos }], country: 5, airlineOffices: [10], agencyOffice: 7, channel: Channel.GDS));

    [Fact]
    public async Task POS_03_the_country_is_optional_on_the_request_and_follows_the_include_flag_when_given()
    {
        var include = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, CountryIsInclude = true, CountryIds = [5] } };
        var unflagged = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, CountryIds = [5] } };
        var exclude = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, CountryIsInclude = false, CountryIds = [116] } };

        Assert.NotEmpty(await AllowedAsync(include, country: 5));
        Assert.Empty(await AllowedAsync(include, country: 6));
        Assert.NotEmpty(await AllowedAsync(include));
        Assert.NotEmpty(await AllowedAsync(unflagged, country: 5));
        Assert.Empty(await AllowedAsync(unflagged, country: 6));
        Assert.Empty(await AllowedAsync(exclude, country: 116));
        Assert.NotEmpty(await AllowedAsync(exclude, country: 5));
        Assert.NotEmpty(await AllowedAsync(exclude));
    }

    [Fact]
    public async Task POS_04_any_airline_office_of_the_caller_satisfies_an_include_and_breaks_an_exclude()
    {
        var include = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, AirlineOfficeIsInclude = true, AirlineOfficeIds = [10] } };
        var exclude = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, AirlineOfficeIsInclude = false, AirlineOfficeIds = [10] } };

        Assert.NotEmpty(await AllowedAsync(include, airlineOffices: [11, 10]));
        Assert.Empty(await AllowedAsync(include, airlineOffices: [11]));
        Assert.Empty(await AllowedAsync(include));
        Assert.Empty(await AllowedAsync(exclude, airlineOffices: [11, 10]));
        Assert.NotEmpty(await AllowedAsync(exclude, airlineOffices: [11]));
        Assert.NotEmpty(await AllowedAsync(exclude));
    }

    [Fact]
    public async Task POS_05_the_travel_agency_office_and_the_channel_are_required_once_the_condition_names_them()
    {
        var office = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, TravelAgencyOfficeIsInclude = true, TravelAgencyOfficeIds = [7] } };
        var notOffice = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, TravelAgencyOfficeIsInclude = false, TravelAgencyOfficeIds = [7] } };
        var channel = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, ChannelIsInclude = true, Channels = ["IBE", "API"] } };
        var notChannel = new[] { new PointOfSaleCondition { PointOfSaleId = Pos, ChannelIsInclude = false, Channels = ["IBE", "API"] } };

        Assert.NotEmpty(await AllowedAsync(office, agencyOffice: 7));
        Assert.Empty(await AllowedAsync(office, agencyOffice: 8));
        Assert.Empty(await AllowedAsync(office));
        Assert.Empty(await AllowedAsync(notOffice, agencyOffice: 7));
        Assert.NotEmpty(await AllowedAsync(notOffice));
        Assert.NotEmpty(await AllowedAsync(channel, channel: Channel.PartnerAPI));
        Assert.NotEmpty(await AllowedAsync(channel, channel: Channel.IBE));
        Assert.Empty(await AllowedAsync(channel, channel: Channel.BackOffice));
        Assert.Empty(await AllowedAsync(notChannel, channel: Channel.IBE));
        Assert.NotEmpty(await AllowedAsync(notChannel, channel: Channel.BackOffice));
    }

    [Fact]
    public async Task POS_06_every_dimension_of_one_condition_must_match_together()
    {
        var conditions = new[]
        {
            new PointOfSaleCondition
            {
                PointOfSaleId = Pos,
                CountryIsInclude = true,
                CountryIds = [5],
                TravelAgencyOfficeIsInclude = true,
                TravelAgencyOfficeIds = [7],
                ChannelIsInclude = true,
                Channels = ["API"]
            }
        };

        Assert.NotEmpty(await AllowedAsync(conditions, country: 5, agencyOffice: 7, channel: Channel.PartnerAPI));
        Assert.Empty(await AllowedAsync(conditions, country: 6, agencyOffice: 7, channel: Channel.PartnerAPI));
        Assert.Empty(await AllowedAsync(conditions, country: 5, agencyOffice: 8, channel: Channel.PartnerAPI));
        Assert.Empty(await AllowedAsync(conditions, country: 5, agencyOffice: 7, channel: Channel.AgencyPanel));
    }

    private sealed class FixedConditions(PointOfSaleConditionSet conditions) : IPointOfSaleConditionReference
    {
        public Task<PointOfSaleConditionSet> GetConditionSetAsync(CancellationToken cancellationToken = default) => Task.FromResult(conditions);
    }
}
