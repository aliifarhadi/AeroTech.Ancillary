using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionPointOfSaleReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionPointOfSaleReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionPointOfSaleReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionPointsOfSale");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
