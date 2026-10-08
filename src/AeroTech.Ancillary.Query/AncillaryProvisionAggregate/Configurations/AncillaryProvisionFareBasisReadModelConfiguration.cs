using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionFareBasisReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionFareBasisReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionFareBasisReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionFareBases");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.FareBasisCode).HasMaxLength(64).IsRequired();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
