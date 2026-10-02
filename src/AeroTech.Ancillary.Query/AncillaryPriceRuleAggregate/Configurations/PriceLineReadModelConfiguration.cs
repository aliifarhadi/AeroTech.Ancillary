using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Configurations
{
    public sealed class PriceLineReadModelConfiguration : IEntityTypeConfiguration<PriceLineReadModel>
    {
        public void Configure(EntityTypeBuilder<PriceLineReadModel> builder)
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
