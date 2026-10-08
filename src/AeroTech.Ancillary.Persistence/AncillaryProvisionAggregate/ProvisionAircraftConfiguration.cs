using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionAircraftConfiguration : IEntityTypeConfiguration<ProvisionAircraft>
    {
        public void Configure(EntityTypeBuilder<ProvisionAircraft> builder)
        {
            builder.ToTable("ProvisionAircraft");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.AircraftId }).IsUnique();
        }
    }
}
