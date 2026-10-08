using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionOperatingAirlineReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionOperatingAirlineReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionOperatingAirlineReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionOperatingAirlines");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
