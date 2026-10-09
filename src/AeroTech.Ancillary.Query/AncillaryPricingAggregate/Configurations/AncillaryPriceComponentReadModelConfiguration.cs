using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Configurations
{
    public sealed class AncillaryPriceComponentReadModelConfiguration : IEntityTypeConfiguration<AncillaryPriceComponentReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryPriceComponentReadModel> builder)
        {
            builder.ToTable("AncillaryPriceComponents");
            builder.HasKey(component => component.Id);
            builder.Property(component => component.Id).ValueGeneratedNever();
            builder.Property(component => component.Code).HasMaxLength(10);
            builder.Property(component => component.Name).HasMaxLength(100);
            builder.Property(component => component.Amount).HasPrecision(19, 6);
            builder.HasIndex(component => component.AncillaryPricingId);
            builder.HasIndex(component => component.AncillaryPricingRateId);
        }
    }
}
