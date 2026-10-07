using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionPriceLineConfiguration : IEntityTypeConfiguration<ProvisionPriceLine>
    {
        public void Configure(EntityTypeBuilder<ProvisionPriceLine> builder)
        {
            builder.ToTable("ProvisionPriceLines");
            builder.HasKey(line => line.Id);
            builder.Property(line => line.Id).ValueGeneratedNever();

            builder.Property(line => line.Code).HasMaxLength(10);
            builder.Property(line => line.Name).HasMaxLength(100);

            builder.HasIndex(line => line.AncillaryProvisionId);
        }
    }
}
