using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Framework.Core.Domain.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer._Shared
{
    public sealed class AncillaryUnitOfWork : IUnitOfWork
    {
        private static readonly string[] CommercialNamespaces =
        [
            typeof(AncillaryServiceDefinition).Namespace!,
            typeof(AncillaryProvision).Namespace!,
            typeof(AncillaryPricing).Namespace!
        ];

        private readonly AncillaryDbContext _commandDbContext;
        private readonly AncillaryQueryDbContext _queryDbContext;

        public AncillaryUnitOfWork(AncillaryDbContext commandDbContext, AncillaryQueryDbContext queryDbContext)
        {
            _commandDbContext = commandDbContext;
            _queryDbContext = queryDbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int affected;

            try
            {
                affected = await _commandDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception) when (exception.Entries.Any(entry => IsCommercial(entry.Entity)))
            {
                throw ExceptionFactory.ConcurrentChangeDetected();
            }
            catch (DbUpdateException exception) when (ToConflict(exception) is { } conflict)
            {
                throw conflict;
            }

            await _queryDbContext.SaveChangesAsync(cancellationToken);

            return affected;
        }

        private static bool IsCommercial(object entity)
            => CommercialNamespaces.Any(
                commercial => entity.GetType().Namespace?.StartsWith(commercial, StringComparison.Ordinal) == true);

        private static BusinessException? ToConflict(DbUpdateException exception)
        {
            if (exception.InnerException is not SqlException { Number: 2601 or 2627 } duplicate)
                return null;

            if (duplicate.Message.Contains("IX_AncillaryPricings_OneActivePerProvision", StringComparison.Ordinal))
                return ExceptionFactory.PricingAlreadyActive();

            if (duplicate.Message.Contains("IX_AncillaryPricings_AncillaryProvisionId_Version", StringComparison.Ordinal))
                return ExceptionFactory.ConcurrentChangeDetected();

            if (duplicate.Message.Contains("IX_AncillaryProvisions_ServiceDefinitionId_Sequence_Active", StringComparison.Ordinal))
                return ExceptionFactory.ProvisionSequenceAlreadyActive(
                    exception.Entries.Select(entry => entry.Entity).OfType<AncillaryProvision>().Select(provision => provision.Sequence).FirstOrDefault());

            if (duplicate.Message.Contains("IX_AncillaryServiceDefinitions_OwnerAirlineId_Ref_Active", StringComparison.Ordinal))
                return ExceptionFactory.ServiceDefinitionRefAlreadyActive(
                    exception.Entries.Select(entry => entry.Entity).OfType<AncillaryServiceDefinition>().Select(definition => definition.ServiceDefinitionRef).FirstOrDefault());

            if (duplicate.Message.Contains("unique index 'IX_Provision", StringComparison.Ordinal))
                return ExceptionFactory.ProvisionConditionAlreadyExists();

            return null;
        }
    }
}
