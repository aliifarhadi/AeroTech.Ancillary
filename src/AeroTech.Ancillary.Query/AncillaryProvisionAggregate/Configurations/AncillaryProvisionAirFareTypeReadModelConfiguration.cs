using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionAirFareTypeReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionAirFareTypeReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionAirFareTypeReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionAirFareTypes");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
