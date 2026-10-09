using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V122ProfileRuleAcceptanceTests
{
    private static readonly string[] SharedSections = ["PassengerEligibility", "SalesRestrictions", "Geography", "TravelDate", "DayTimeApplication", "AdvancePurchase"];

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V122ProfileRuleAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task<(long DefinitionId, long ProvisionId)> DraftRuleAsync(string code)
    {
        var variant = V122Catalog.Case(code);
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(
            V122Catalog.NeedsExternalSupplier(variant) ? M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider) : M1Commands.LocalSupplier(airlineId));
        var definition = await _proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, supplierId));
        var provision = await RequestAsync(scope => scope.DefineProvision.DefineAsync(V122Catalog.Rule(variant, definition.Id) with
        {
            PetRule = null,
            AssistedTravelRule = null,
            AirportServiceRule = null
        }));

        return (definition.Id, provision.Id);
    }

    private Task<BackofficeProvisionDto> ReadAsync(long provisionId) => RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId));

    [Fact]
    public async Task V122_a_pet_rule_is_edited_and_removed_on_a_draft_of_a_pet_product_only()
    {
        var (_, pet) = await DraftRuleAsync("A13");
        var (_, priority) = await DraftRuleAsync("A23");
        var rule = new ProvisionPetRuleInput("EU_ENTRY", 16, 6m, ConfirmationRequirement.SubjectToConfirmation);

        Assert.Null((await ReadAsync(pet)).PetRule);

        await RequestAsync(scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(pet, rule)));

        var authored = (await ReadAsync(pet)).PetRule!;

        Assert.Equal(("EU_ENTRY", 16, 6m, "SubjectToConfirmation"), (authored.CountryExceptionCode, authored.MinAnimalAgeWeeksOverride!.Value, authored.MaxCombinedKgOverride!.Value, authored.AcceptanceMode.Name));

        await RequestAsync(scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(pet, rule with { MaxCombinedKgOverride = 5m })));

        var edited = (await ReadAsync(pet)).PetRule!;

        Assert.Equal((authored.Id, 5m), (edited.Id, edited.MaxCombinedKgOverride!.Value));
        await RefusedAsync(16321, 409, scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(priority, rule)));
        await RefusedAsync(16302, 422, scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(pet, rule with { MaxCombinedKgOverride = 0m })));
        await RefusedAsync(16301, 404, scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(999_999_999, rule)));
        await RequestAsync(scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(pet, null)));
        await RequestAsync(scope => scope.ChangePetRule.ChangeAsync(new TestChangeProvisionPetRuleCommand(priority, null)));

        Assert.Null((await ReadAsync(pet)).PetRule);
        Assert.Equal(
            new[] { "0" },
            await RequestAsync(async scope => new[] { (await scope.Command.Database.SqlQuery<string>($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.ProvisionPetRules WHERE AncillaryProvisionId = {pet}").ToListAsync()).Single() }));
    }

    [Fact]
    public async Task V122_an_assisted_travel_rule_and_an_airport_service_rule_follow_their_own_profile_and_the_draft_lifecycle()
    {
        var (_, medical) = await DraftRuleAsync("A17");
        var (_, lounge) = await DraftRuleAsync("A20");
        var assisted = new ProvisionAssistedTravelRuleInput(2880, null, true);
        var airport = new ProvisionAirportServiceRuleInput("T1", AirportServiceDirection.Departure, new TimeOnly(6, 0), new TimeOnly(20, 0), null, 1);

        await RequestAsync(scope => scope.ChangeAssistedTravelRule.ChangeAsync(new TestChangeProvisionAssistedTravelRuleCommand(medical, assisted)));
        await RequestAsync(scope => scope.ChangeAirportServiceRule.ChangeAsync(new TestChangeProvisionAirportServiceRuleCommand(lounge, airport)));
        await RefusedAsync(16321, 409, scope => scope.ChangeAssistedTravelRule.ChangeAsync(new TestChangeProvisionAssistedTravelRuleCommand(lounge, assisted)));
        await RefusedAsync(16321, 409, scope => scope.ChangeAirportServiceRule.ChangeAsync(new TestChangeProvisionAirportServiceRuleCommand(medical, airport)));
        await RefusedAsync(16302, 422, scope => scope.ChangeAirportServiceRule.ChangeAsync(new TestChangeProvisionAirportServiceRuleCommand(lounge, airport with { MaxGuestsPerPrimary = -1 })));

        var medicalRule = (await ReadAsync(medical)).AssistedTravelRule!;
        var loungeRule = (await ReadAsync(lounge)).AirportServiceRule!;

        Assert.Equal((2880, true), (medicalRule.MinimumLeadTimeMinutes!.Value, medicalRule.MedicalApprovalRequired!.Value));
        Assert.Null(medicalRule.ConnectionPolicy);
        Assert.Equal(("T1", "Departure", new TimeOnly(6, 0), new TimeOnly(20, 0), 1), (loungeRule.TerminalRef, loungeRule.Direction!.Name, loungeRule.ServiceWindowStart!.Value, loungeRule.ServiceWindowEnd!.Value, loungeRule.MaxGuestsPerPrimary!.Value));

        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(medical)));

        await RefusedAsync(16303, 409, scope => scope.ChangeAssistedTravelRule.ChangeAsync(new TestChangeProvisionAssistedTravelRuleCommand(medical, null)));
        Assert.Equal("Active", (await ReadAsync(medical)).Status.Name);
        Assert.NotNull((await ReadAsync(medical)).AssistedTravelRule);
    }

    [Fact]
    public async Task V122_a_definition_is_edited_only_as_its_own_profile_and_a_draft_may_move_to_another_variant_of_that_profile()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var cabin = V122Catalog.Define(V122Catalog.Case("A13"), airlineId, supplierId);
        var draft = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(cabin));
        var asBaggage = V122Catalog.Define(V122Catalog.Case("A01"), airlineId, supplierId) with { ServiceDefinitionRef = cabin.ServiceDefinitionRef };
        var asHold = V122Catalog.Define(V122Catalog.Case("A14"), airlineId, supplierId) with { ServiceDefinitionRef = cabin.ServiceDefinitionRef };

        await RefusedAsync(16219, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(P1Commands.Change(draft.Id, asBaggage)));

        var unchanged = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(draft.Id));

        Assert.Equal(("Pet", "A13", "Cabin"), (unchanged.Profile!.Name, unchanged.VariantCode, unchanged.Specification!.Pet!.TransportMode.Name));
        Assert.Null(unchanged.Specification.Baggage);

        await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(P1Commands.Change(draft.Id, asHold)));

        var moved = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(draft.Id));

        Assert.Equal(("Pet", "A14", "Hold", 2), (moved.Profile!.Name, moved.VariantCode, moved.Specification!.Pet!.TransportMode.Name, moved.Specification.Pet.AllowedHoldAnimalSizeBrackets.Count));
    }

    [Theory]
    [InlineData("A01", "FlightApplication,FareApplication,BaggageApplication")]
    [InlineData("A07", "FlightApplication,FareApplication,SeatApplication")]
    [InlineData("A11", "FlightApplication,FareApplication")]
    [InlineData("A13", "FlightApplication,FareApplication,PetRule")]
    [InlineData("A19", "FlightApplication,FareApplication,AssistedTravelRule")]
    [InlineData("A20", "FlightApplication,FareApplication,AirportServiceRule")]
    [InlineData("A22", "AirportServiceRule")]
    [InlineData("A24", "FlightApplication,FareApplication")]
    public async Task V122_the_provision_detail_names_only_the_rule_sections_of_its_profile(string code, string own)
    {
        var (_, provisionId) = await DraftRuleAsync(code);
        var detail = await ReadAsync(provisionId);

        Assert.Equal((V122Catalog.Case(code).Profile.ToString(), code), (detail.Profile!.Name, detail.VariantCode));
        Assert.Equal(SharedSections.Concat(own.Split(',')), detail.ApplicableRuleSections);
    }
}
