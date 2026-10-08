using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryPricingAggregate
{
    public sealed class AncillaryPricingConfiguration : IEntityTypeConfiguration<AncillaryPricing>
    {
        public void Configure(EntityTypeBuilder<AncillaryPricing> builder)
        {
            builder.ToTable("AncillaryPricings");
            builder.HasKey(pricing => pricing.Id);
            builder.Property(pricing => pricing.Id).ValueGeneratedNever();

            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(pricing => pricing.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(pricing => pricing.PriceLines)
                .WithOne()
                .HasForeignKey(line => line.AncillaryPricingId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(pricing => pricing.PriceLines).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(pricing => new { pricing.AncillaryProvisionId, pricing.Version }).IsUnique();
            builder.HasIndex(
                    pricing => new { pricing.AncillaryProvisionId, pricing.Status },
                    "IX_AncillaryPricings_OneActivePerProvision")
                .IsUnique()
                .HasFilter($"[Status] = {(int)PricingStatus.Active}");
            builder.HasIndex(pricing => pricing.Status);
        }
    }
}
