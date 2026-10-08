using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Configurations
{
    public sealed class AncillaryPricingReadModelConfiguration : IEntityTypeConfiguration<AncillaryPricingReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryPricingReadModel> builder)
        {
            builder.ToTable("AncillaryPricings");
            builder.HasKey(pricing => pricing.Id);
            builder.Property(pricing => pricing.Id).ValueGeneratedNever();
            builder.HasIndex(pricing => new { pricing.AncillaryProvisionId, pricing.Status });
        }
    }
}
