using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionRoutePairConfiguration : IEntityTypeConfiguration<ProvisionRoutePair>
    {
        public void Configure(EntityTypeBuilder<ProvisionRoutePair> builder)
        {
            builder.ToTable("ProvisionRoutePairs");
            builder.HasKey(pair => pair.Id);
            builder.Property(pair => pair.Id).ValueGeneratedNever();

            builder.HasIndex(pair => pair.AncillaryProvisionId);
        }
    }
}
