using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionPriceLineReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionPriceLineReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionPriceLineReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionPriceLines");
            builder.HasKey(line => line.Id);
            builder.Property(line => line.Id).ValueGeneratedNever();

            builder.Property(line => line.Code).HasMaxLength(10);
            builder.Property(line => line.Name).HasMaxLength(100);

            builder.HasIndex(line => line.AncillaryProvisionId);
        }
    }
}
