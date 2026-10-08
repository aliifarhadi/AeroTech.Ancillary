using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionOriginAirportConfiguration : IEntityTypeConfiguration<ProvisionOriginAirport>
    {
        public void Configure(EntityTypeBuilder<ProvisionOriginAirport> builder)
        {
            builder.ToTable("ProvisionOriginAirports");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.AirportId }).IsUnique();
        }
    }
}
