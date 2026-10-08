using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record FamilyProofResult(long ServiceDefinitionId, long ProvisionId, long? PricingId);

public sealed record PublishedRule(BackofficeProvisionDto Provision, BackofficePricingDto? Pricing);

public sealed record FamilyPricingProof(
    Func<long, TestDefinePricingCommand> Draft,
    Func<long, TestDefinePricingCommand> Edited,
    Action<BackofficePricingDto> AssertDraft,
    Action<BackofficePricingDto> AssertEdited);

public sealed class FamilyProof
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock;

    public FamilyProof(TestDatabase database, FixedClock clock)
    {
        _database = database;
        _clock = clock;
    }

    public async Task<long> SupplierAsync(TestRegisterSupplierCommand supplier)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return (await scope.RegisterSupplier.RegisterAsync(supplier)).Id;
    }

    public async Task<BackofficeServiceDefinitionDto> DefinitionAsync(TestDefineServiceDefinitionCommand definition, bool activate = true)
    {
        long definitionId;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            definitionId = (await author.DefineServiceDefinition.DefineAsync(definition)).Id;

            if (activate)
                await author.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId));
        }

        await using var reader = new AncillaryScope(_database, _clock);

        return await reader.GetServiceDefinitionById.ExecuteAsync(definitionId);
    }

    public async Task<PublishedRule> RuleAsync(
        TestDefineProvisionCommand provision,
        Func<long, TestDefinePricingCommand>? pricing = null,
        bool publish = true)
    {
        long provisionId;
        long? pricingId = null;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            provisionId = (await author.DefineProvision.DefineAsync(provision)).Id;

            if (pricing is not null)
                pricingId = (await author.DefinePricing.DefineAsync(pricing(provisionId))).Id;
        }

        if (publish)
        {
            await using var publisher = new AncillaryScope(_database, _clock);

            if (pricingId is { } id)
                await publisher.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, id));
            else
                await publisher.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId));
        }

        await using var reader = new AncillaryScope(_database, _clock);

        return new PublishedRule(
            await reader.GetProvisionById.ExecuteAsync(provisionId),
            pricingId is { } readId ? await reader.GetPricingById.ExecuteAsync(readId) : null);
    }

    public async Task<IReadOnlyList<ProvisionPaginatedRowDto>> ListedProvisionsAsync(long serviceDefinitionId)
    {
        await using var reader = new AncillaryScope(_database, _clock);

        return (await reader.GetProvisionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = serviceDefinitionId, PageSize = 50 }))
            .Results.ToList();
    }

    public async Task<FamilyProofResult> ProveAsync(
        TestDefineServiceDefinitionCommand definition,
        Func<long, TestDefineProvisionCommand> provision,
        Func<TestDefineProvisionCommand, TestDefineProvisionCommand> edit,
        Action<BackofficeServiceDefinitionDto> assertDefinition,
        Action<BackofficeProvisionDto> assertDraft,
        Action<BackofficeProvisionDto> assertEdited,
        FamilyPricingProof? pricing = null)
    {
        long definitionId;
        long provisionId;
        long? pricingId = null;
        TestDefineProvisionCommand draftCommand;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            var createdDefinition = await author.DefineServiceDefinition.DefineAsync(definition);

            Assert.Equal((ServiceDefinitionStatus.Draft, definition.PricingUnit), (createdDefinition.Status, createdDefinition.PricingUnit!.Value));

            definitionId = createdDefinition.Id;
            draftCommand = provision(definitionId);

            var createdProvision = await author.DefineProvision.DefineAsync(draftCommand);

            Assert.Equal(ProvisionStatus.Draft, createdProvision.Status);

            provisionId = createdProvision.Id;

            if (pricing is not null)
            {
                var createdPricing = await author.DefinePricing.DefineAsync(pricing.Draft(provisionId));

                Assert.Equal((PricingStatus.Draft, 1, definition.PricingUnit), (createdPricing.Status, createdPricing.Version, createdPricing.PricingUnit!.Value));

                pricingId = createdPricing.Id;
            }
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var definitionDetail = await reader.GetServiceDefinitionById.ExecuteAsync(definitionId);
            var provisionDetail = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(
                ("Draft", definition.ServiceDefinitionRef, definition.CommercialName, definition.PricingUnit.ToString()),
                (definitionDetail.Status.Name, definitionDetail.ServiceDefinitionRef, definitionDetail.CommercialName, definitionDetail.PricingUnit!.Name));
            Assert.Equal(("Draft", definitionId), (provisionDetail.Status.Name, provisionDetail.ServiceDefinitionId));
            assertDefinition(definitionDetail);
            assertDraft(provisionDetail);

            var listedDefinitions = await reader.GetServiceDefinitionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery
                {
                    OwnerAirlineId = definition.OwnerAirlineId,
                    ServiceDefinitionRef = definition.ServiceDefinitionRef
                });
            var listedProvisions = await reader.GetProvisionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definitionId });

            Assert.Equal((definitionId.ToString(), "Draft"), listedDefinitions.Results.Select(row => (row.Id, row.Status.Name)).Single());
            Assert.Equal((provisionId.ToString(), "Draft"), listedProvisions.Results.Select(row => (row.Id, row.Status.Name)).Single());

            if (pricingId is { } draftPricingId)
            {
                var pricingDetail = await reader.GetPricingById.ExecuteAsync(draftPricingId);
                var listedPricings = await reader.GetPricingsPaginated.ExecuteAsync(
                    new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = provisionId });

                Assert.Equal(("Draft", provisionId), (pricingDetail.Status.Name, pricingDetail.AncillaryProvisionId));
                Assert.Equal((draftPricingId.ToString(), "Draft"), listedPricings.Results.Select(row => (row.Id, row.Status.Name)).Single());
                pricing!.AssertDraft(pricingDetail);
            }
        }

        var editedDefinition = definition with { Description = $"{definition.CommercialName} (edited)" };
        var editedCommand = edit(draftCommand);

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await editor.ChangeServiceDefinition.ChangeAsync(P1Commands.Change(definitionId, editedDefinition));
            await editor.ChangeProvision.ChangeAsync(P1Commands.Change(provisionId, editedCommand));

            if (pricing is not null && pricingId is { } changedPricingId)
                await editor.ChangePricing.ChangeAsync(V12Commands.Change(changedPricingId, pricing.Edited(provisionId)));
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var definitionDetail = await reader.GetServiceDefinitionById.ExecuteAsync(definitionId);
            var provisionDetail = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(("Draft", editedDefinition.Description), (definitionDetail.Status.Name, definitionDetail.Description));
            Assert.Equal("Draft", provisionDetail.Status.Name);
            assertDefinition(definitionDetail);
            assertEdited(provisionDetail);

            if (pricing is not null && pricingId is { } editedPricingId)
            {
                var pricingDetail = await reader.GetPricingById.ExecuteAsync(editedPricingId);

                Assert.Equal(("Draft", 1), (pricingDetail.Status.Name, pricingDetail.Version));
                pricing.AssertEdited(pricingDetail);
            }
        }

        _clock.Now = _clock.Now.AddHours(1);

        await using (var publisher = new AncillaryScope(_database, _clock))
        {
            Assert.Equal(
                ServiceDefinitionStatus.Active,
                (await publisher.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId))).Status);
        }

        await using (var publisher = new AncillaryScope(_database, _clock))
        {
            var published = pricingId is { } publishedPricingId
                ? await publisher.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, publishedPricingId))
                : await publisher.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId));

            Assert.Equal(ProvisionStatus.Active, published.Status);
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var definitionDetail = await reader.GetServiceDefinitionById.ExecuteAsync(definitionId);
            var provisionDetail = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(("Active", _clock.Now, editedDefinition.Description), (definitionDetail.Status.Name, definitionDetail.ActivatedAt!.Value, definitionDetail.Description));
            Assert.Equal(("Active", _clock.Now), (provisionDetail.Status.Name, provisionDetail.ActivatedAt!.Value));
            assertDefinition(definitionDetail);
            assertEdited(provisionDetail);

            var activeDefinitions = await reader.GetServiceDefinitionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery
                {
                    OwnerAirlineId = definition.OwnerAirlineId,
                    ServiceDefinitionRef = definition.ServiceDefinitionRef,
                    Status = ServiceDefinitionStatus.Active
                });
            var activeProvisions = await reader.GetProvisionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definitionId, Status = ProvisionStatus.Active });

            Assert.Equal(definitionId.ToString(), activeDefinitions.Results.Single().Id);
            Assert.Equal(provisionId.ToString(), activeProvisions.Results.Single().Id);
            Assert.Equal(
                ProvisionStatus.Active,
                (await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(row => row.Id == provisionId)).Status);

            var activePricings = await reader.GetPricingsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = provisionId, Status = PricingStatus.Active });

            if (pricingId is { } activePricingId)
            {
                var pricingDetail = await reader.GetPricingById.ExecuteAsync(activePricingId);

                Assert.Equal(("Active", _clock.Now), (pricingDetail.Status.Name, pricingDetail.ActivatedAt!.Value));
                Assert.Equal(activePricingId.ToString(), activePricings.Results.Single().Id);
                pricing!.AssertEdited(pricingDetail);
            }
            else
            {
                Assert.Empty(activePricings.Results);
            }
        }

        return new FamilyProofResult(definitionId, provisionId, pricingId);
    }
}
