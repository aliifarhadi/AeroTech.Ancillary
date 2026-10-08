using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionRoutePairConfiguration : IEntityTypeConfiguration<ProvisionRoutePair>
    {
        public void Configure(EntityTypeBuilder<ProvisionRoutePair> builder)
        {
            builder.ToTable("ProvisionRoutePairs");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.OriginAirportId, row.DestinationAirportId }).IsUnique();
        }
    }
}
