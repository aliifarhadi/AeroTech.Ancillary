using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionPointOfSaleConfiguration : IEntityTypeConfiguration<ProvisionPointOfSale>
    {
        public void Configure(EntityTypeBuilder<ProvisionPointOfSale> builder)
        {
            builder.ToTable("ProvisionPointsOfSale");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.PointOfSaleId }).IsUnique();
        }
    }
}
