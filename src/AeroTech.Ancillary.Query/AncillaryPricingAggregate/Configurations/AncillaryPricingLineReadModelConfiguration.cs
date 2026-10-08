using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Configurations
{
    public sealed class AncillaryPricingLineReadModelConfiguration : IEntityTypeConfiguration<AncillaryPricingLineReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryPricingLineReadModel> builder)
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
