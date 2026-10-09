using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryPricingAggregate
{
    public sealed class AncillaryPriceComponentConfiguration : IEntityTypeConfiguration<AncillaryPriceComponent>
    {
        public const string ComponentKeyIndex = "IX_AncillaryPriceComponents_ComponentKey";

        public void Configure(EntityTypeBuilder<AncillaryPriceComponent> builder)
        {
            builder.ToTable("AncillaryPriceComponents");
            builder.HasKey(component => component.Id);
            builder.Property(component => component.Id).ValueGeneratedNever();
            builder.Property(component => component.Code).HasMaxLength(10);
            builder.Property(component => component.Name).HasMaxLength(100);
            builder.OwnsOne(component => component.Amount, money =>
            {
                money.Property(value => value.Amount).HasColumnName("Amount").HasPrecision(19, 6);
                money.Property(value => value.CurrencyId).HasColumnName("CurrencyId");
            });
            builder.Navigation(component => component.Amount).IsRequired();

            builder.HasIndex(
                    component => new
                    {
                        component.AncillaryPricingRateId,
                        component.Category,
                        component.Code,
                        component.CountryId,
                        component.StationAirportId,
                        component.FeeApplicationUnit
                    },
                    ComponentKeyIndex)
                .IsUnique()
                .HasFilter(null);
        }
    }
}
