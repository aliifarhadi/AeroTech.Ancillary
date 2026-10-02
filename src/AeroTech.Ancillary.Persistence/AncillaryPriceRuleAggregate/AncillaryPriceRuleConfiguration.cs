using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryPriceRuleAggregate
{
    public sealed class AncillaryPriceRuleConfiguration : IEntityTypeConfiguration<AncillaryPriceRule>
    {
        public void Configure(EntityTypeBuilder<AncillaryPriceRule> builder)
        {
            builder.ToTable("AncillaryPriceRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();

            builder.Property(rule => rule.ProductRef).HasMaxLength(20).IsRequired();

            builder.OwnsOne(rule => rule.Conditions, conditions =>
            {
                conditions.PrimitiveCollection(value => value.PassengerTypes)
                    .HasField("_passengerTypes")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("PassengerTypes");
                conditions.PrimitiveCollection(value => value.OriginAirportIds)
                    .HasField("_originAirportIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("OriginAirportIds");
                conditions.PrimitiveCollection(value => value.DestinationAirportIds)
                    .HasField("_destinationAirportIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("DestinationAirportIds");
            });

            builder.Navigation(rule => rule.Conditions).IsRequired();

            builder.HasMany(rule => rule.Lines)
                .WithOne()
                .HasForeignKey(line => line.AncillaryPriceRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(rule => rule.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(rule => new { rule.OwnerAirlineId, rule.ProductRef, rule.CurrencyId, rule.Priority })
                .IsUnique()
                .HasFilter($"[Status] = {(int)AncillaryPriceRuleStatus.Active}");
            builder.HasIndex(rule => new { rule.CurrencyId, rule.Status });
        }
    }
}
