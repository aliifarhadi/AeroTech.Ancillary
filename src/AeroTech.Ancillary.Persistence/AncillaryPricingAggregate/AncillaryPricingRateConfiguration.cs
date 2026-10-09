using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryPricingAggregate
{
    public sealed class AncillaryPricingRateConfiguration : IEntityTypeConfiguration<AncillaryPricingRate>
    {
        public const string RateKeyIndex = "IX_AncillaryPricingRates_RateKey";

        public void Configure(EntityTypeBuilder<AncillaryPricingRate> builder)
        {
            builder.ToTable("AncillaryPricingRates");
            builder.HasKey(rate => rate.Id);
            builder.Property(rate => rate.Id).ValueGeneratedNever();
            builder.Property(rate => rate.BaseAmount).HasPrecision(19, 6);
            builder.Ignore(rate => rate.BasePrice);
            builder.Ignore(rate => rate.UnitTotal);
            builder.Ignore(rate => rate.UnappliedFees);
            builder.Ignore(rate => rate.IsUnitTotalComplete);

            builder.HasMany(rate => rate.Components)
                .WithOne()
                .HasForeignKey(component => component.AncillaryPricingRateId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(rate => rate.Components).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(
                    rate => new { rate.AncillaryPricingId, rate.CurrencyId, rate.PassengerTypeCode, rate.AgeFromInclusive, rate.AgeToExclusive },
                    RateKeyIndex)
                .IsUnique()
                .HasFilter(null);
        }
    }
}
