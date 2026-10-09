using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Configurations
{
    public sealed class AncillaryPricingRateReadModelConfiguration : IEntityTypeConfiguration<AncillaryPricingRateReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryPricingRateReadModel> builder)
        {
            builder.ToTable("AncillaryPricingRates");
            builder.HasKey(rate => rate.Id);
            builder.Property(rate => rate.Id).ValueGeneratedNever();
            builder.Property(rate => rate.BaseAmount).HasPrecision(19, 6);
            builder.HasIndex(rate => rate.AncillaryPricingId);
        }
    }
}
