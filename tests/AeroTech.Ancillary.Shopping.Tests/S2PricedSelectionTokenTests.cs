using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2PricedSelectionTokenTests
{
    private static readonly DateTimeOffset ExpiresAtUtc = Now.AddMinutes(30);

    private static PricedSelection Selection()
        => new()
        {
            SourceKind = ShoppingSourceKind.Order,
            OrderId = 11,
            OrderCommercialVersion = 3,
            SourceVersion = "source-version-1",
            Scope = new AncillarySalesScope(Airline, Channel.PartnerAPI, [Pos], TravelAgencyId: 77, CustomerId: 5001),
            Kind = SelectionLineKind.Ancillary,
            OfferItemRef = "offer-item-ref",
            Selection = new AncillarySelection
            {
                Quantity = 2,
                MealCode = "VGML",
                CarrierDimensions = new SelectedDimensions(55m, 40m, 23m),
                DocumentAcknowledgements = ["ENTRY_RULES_ACK"],
                GuardianHandoffContactProvided = true
            },
            CoverageScope = ServiceCoverageScope.Portion,
            TravellerRefs = ["T1"],
            FlightRefs = ["F1"],
            PortionRefs = ["P1"],
            ServiceDefinitionRef = "BAG_23",
            DefinitionVersionId = 101,
            ProvisionId = 202,
            PricingRevisionId = 303,
            CurrencyId = Eur,
            Total = 224.125m,
            Units = 2
        };

    private static FixedClock Clock() => new() { Now = Now };

    [Fact]
    public async Task SEC_01_a_priced_selection_reads_back_exactly_and_one_changed_character_rejects_it()
    {
        var tokens = SelectionDesk.Tokens(Clock());
        var issued = tokens.Issue(Selection(), ExpiresAtUtc);
        var reading = await tokens.ReadAsync(issued.Value);
        var parts = issued.Value.Split('.');
        var payload = parts[1].ToCharArray();

        payload[payload.Length / 2] = payload[payload.Length / 2] == 'A' ? 'B' : 'A';

        Assert.Equal((SelectionResolutionStatus.Valid, (DateTimeOffset?)ExpiresAtUtc), (reading.Status, reading.ExpiresAtUtc));
        Assert.Equivalent(Selection(), reading.Selection, strict: true);
        Assert.Equal(PricedSelectionReading.Rejected, await tokens.ReadAsync(string.Join('.', parts[0], new string(payload), parts[2])));
        Assert.Equal(PricedSelectionReading.Rejected, await tokens.ReadAsync(string.Join('.', parts[0], parts[1], parts[2][..^2] + (parts[2][^2..] == "AA" ? "BB" : "AA"))));
    }

    [Fact]
    public async Task SEC_02_a_selection_signed_with_another_key_or_without_any_signature_is_rejected()
    {
        var foreign = SelectionDesk.Tokens(Clock(), "another-ancillary-signing-key-of-sufficient-length").Issue(Selection(), ExpiresAtUtc);
        var unsigned = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "AeroTech.Ancillary",
            Audience = "AeroTech.Ancillary",
            IssuedAt = Now.UtcDateTime,
            NotBefore = Now.UtcDateTime,
            Expires = ExpiresAtUtc.UtcDateTime,
            Claims = new Dictionary<string, object> { ["selection"] = "{}" }
        });
        var tokens = SelectionDesk.Tokens(Clock());

        Assert.Equal(PricedSelectionReading.Rejected, await tokens.ReadAsync(foreign.Value));
        Assert.Equal(PricedSelectionReading.Rejected, await tokens.ReadAsync(unsigned));
    }

    [Fact]
    public async Task SEC_03_an_expired_selection_reads_as_expired_and_is_never_renewed()
    {
        var clock = Clock();
        var tokens = SelectionDesk.Tokens(clock);
        var issued = tokens.Issue(Selection(), ExpiresAtUtc);

        clock.Now = ExpiresAtUtc;

        var reading = await tokens.ReadAsync(issued.Value);

        Assert.Equal((SelectionResolutionStatus.Expired, (DateTimeOffset?)ExpiresAtUtc, 11L), (reading.Status, reading.ExpiresAtUtc, reading.Selection!.OrderId));
    }

    [Fact]
    public async Task SEC_04_a_selection_read_before_its_issue_time_is_rejected()
    {
        var clock = Clock();
        var tokens = SelectionDesk.Tokens(clock);
        var issued = tokens.Issue(Selection(), ExpiresAtUtc);

        clock.Now = Now.AddMinutes(-1);

        Assert.Equal(PricedSelectionReading.Rejected, await tokens.ReadAsync(issued.Value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-token")]
    [InlineData("a.b.c")]
    public async Task SEC_24_empty_or_malformed_text_is_rejected(string? text)
        => Assert.Equal(PricedSelectionReading.Rejected, await SelectionDesk.Tokens(Clock()).ReadAsync(text));

    [Fact]
    public async Task SEC_24_an_oversize_token_is_rejected_before_it_is_parsed()
        => Assert.Equal(PricedSelectionReading.Rejected, await SelectionDesk.Tokens(Clock()).ReadAsync(new string('a', 8193)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short-signing-key")]
    public async Task S2_TOKEN_a_missing_or_short_signing_key_fails_closed_on_issue_and_on_read(string? signingKey)
    {
        var issued = SelectionDesk.Tokens(Clock()).Issue(Selection(), ExpiresAtUtc);
        var unconfigured = SelectionDesk.Tokens(Clock(), signingKey);

        Assert.Throws<InvalidOperationException>(() => unconfigured.Issue(Selection(), ExpiresAtUtc));
        await Assert.ThrowsAsync<InvalidOperationException>(() => unconfigured.ReadAsync(issued.Value));
    }
}
