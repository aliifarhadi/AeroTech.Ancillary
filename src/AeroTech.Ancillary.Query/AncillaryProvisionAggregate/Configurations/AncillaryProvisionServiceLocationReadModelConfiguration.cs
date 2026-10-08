using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionServiceLocationReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionServiceLocationReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionServiceLocationReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionServiceLocations");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
