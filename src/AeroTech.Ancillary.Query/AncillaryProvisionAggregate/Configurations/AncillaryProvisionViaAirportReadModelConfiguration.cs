using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionViaAirportReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionViaAirportReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionViaAirportReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionViaAirports");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
