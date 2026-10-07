using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record FamilyProofResult(long ServiceDefinitionId, long ProvisionId);

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

    public async Task<BackofficeServiceDefinitionDto> DefinitionAsync(TestDefineServiceDefinitionCommand definition, bool activate)
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

    public async Task<BackofficeProvisionDto> ProvisionAsync(TestDefineProvisionCommand provision, bool activate)
    {
        long provisionId;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            provisionId = (await author.DefineProvision.DefineAsync(provision)).Id;

            if (activate)
                await author.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId));
        }

        await using var reader = new AncillaryScope(_database, _clock);

        return await reader.GetProvisionById.ExecuteAsync(provisionId);
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
        Action<BackofficeProvisionDto> assertEdited)
    {
        long definitionId;
        long provisionId;
        TestDefineProvisionCommand draftCommand;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            var createdDefinition = await author.DefineServiceDefinition.DefineAsync(definition);

            Assert.Equal(ServiceDefinitionStatus.Draft, createdDefinition.Status);

            definitionId = createdDefinition.Id;
            draftCommand = provision(definitionId);

            var createdProvision = await author.DefineProvision.DefineAsync(draftCommand);

            Assert.Equal(ProvisionStatus.Draft, createdProvision.Status);

            provisionId = createdProvision.Id;
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var definitionDetail = await reader.GetServiceDefinitionById.ExecuteAsync(definitionId);
            var provisionDetail = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(("Draft", definition.ServiceDefinitionRef, definition.CommercialName), (definitionDetail.Status.Name, definitionDetail.ServiceDefinitionRef, definitionDetail.CommercialName));
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
        }

        var editedDefinition = definition with { Description = $"{definition.CommercialName} (edited)" };
        var editedCommand = edit(draftCommand);

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await editor.ChangeServiceDefinition.ChangeAsync(P1Commands.Change(definitionId, editedDefinition));
            await editor.ChangeProvision.ChangeAsync(P1Commands.Change(provisionId, editedCommand));
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var definitionDetail = await reader.GetServiceDefinitionById.ExecuteAsync(definitionId);
            var provisionDetail = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(("Draft", editedDefinition.Description), (definitionDetail.Status.Name, definitionDetail.Description));
            Assert.Equal("Draft", provisionDetail.Status.Name);
            assertDefinition(definitionDetail);
            assertEdited(provisionDetail);
        }

        _clock.Now = _clock.Now.AddHours(1);

        await using (var publisher = new AncillaryScope(_database, _clock))
        {
            Assert.Equal(
                ServiceDefinitionStatus.Active,
                (await publisher.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId))).Status);
            Assert.Equal(
                ProvisionStatus.Active,
                (await publisher.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId))).Status);
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
                ServiceDefinitionStatus.Active,
                (await reader.Query.AncillaryServiceDefinitions.AsNoTracking().SingleAsync(row => row.Id == definitionId)).Status);
            Assert.Equal(
                ProvisionStatus.Active,
                (await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(row => row.Id == provisionId)).Status);
        }

        return new FamilyProofResult(definitionId, provisionId);
    }
}
