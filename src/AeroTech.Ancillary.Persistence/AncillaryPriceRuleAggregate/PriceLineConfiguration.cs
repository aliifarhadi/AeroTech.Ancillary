using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryPriceRuleAggregate
{
    public sealed class PriceLineConfiguration : IEntityTypeConfiguration<PriceLine>
    {
        public void Configure(EntityTypeBuilder<PriceLine> builder)
        {
            builder.ToTable("PriceLines");
            builder.HasKey(line => line.Id);
            builder.Property(line => line.Id).ValueGeneratedNever();

            builder.Property(line => line.Code).HasMaxLength(10);
            builder.Property(line => line.Name).HasMaxLength(100);

            builder.HasIndex(line => line.AncillaryPriceRuleId);
        }
    }
}
