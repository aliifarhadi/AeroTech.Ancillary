using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionFareFamilyReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionFareFamilyReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionFareFamilyReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionFareFamilies");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
