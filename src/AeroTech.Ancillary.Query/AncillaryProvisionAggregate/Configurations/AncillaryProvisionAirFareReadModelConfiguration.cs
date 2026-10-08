using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionAirFareReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionAirFareReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionAirFareReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionAirFares");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
