using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionOriginAirportReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionOriginAirportReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionOriginAirportReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionOriginAirports");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
