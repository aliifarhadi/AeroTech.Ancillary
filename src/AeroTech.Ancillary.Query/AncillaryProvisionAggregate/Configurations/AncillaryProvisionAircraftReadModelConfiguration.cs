using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionAircraftReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionAircraftReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionAircraftReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionAircraft");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
