using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryPricingAggregate
{
    public sealed class AncillaryPricingLineConfiguration : IEntityTypeConfiguration<AncillaryPricingLine>
    {
        public void Configure(EntityTypeBuilder<AncillaryPricingLine> builder)
        {
            builder.ToTable("AncillaryPricingLines");
            builder.HasKey(line => line.Id);
            builder.Property(line => line.Id).ValueGeneratedNever();
            builder.Property(line => line.Code).HasMaxLength(10);
            builder.Property(line => line.Name).HasMaxLength(100);
            builder.HasIndex(line => line.AncillaryPricingId);
        }
    }
}
