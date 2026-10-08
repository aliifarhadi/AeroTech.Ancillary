using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionDestinationAirportReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionDestinationAirportReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionDestinationAirportReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionDestinationAirports");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
