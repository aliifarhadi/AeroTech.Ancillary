using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Configurations
{
    public sealed class AncillaryPriceRuleReadModelConfiguration : IEntityTypeConfiguration<AncillaryPriceRuleReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryPriceRuleReadModel> builder)
        {
            builder.ToTable("AncillaryPriceRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();

            builder.Property(rule => rule.ProductRef).HasMaxLength(20).IsRequired();

            builder.PrimitiveCollection(rule => rule.PassengerTypes);
            builder.PrimitiveCollection(rule => rule.OriginAirportIds);
            builder.PrimitiveCollection(rule => rule.DestinationAirportIds);

            builder.HasIndex(rule => new { rule.OwnerAirlineId, rule.ProductRef, rule.CurrencyId, rule.Priority });
        }
    }
}
