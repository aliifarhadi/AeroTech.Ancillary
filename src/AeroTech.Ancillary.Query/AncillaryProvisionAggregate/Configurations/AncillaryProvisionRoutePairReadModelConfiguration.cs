using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionRoutePairReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionRoutePairReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionRoutePairReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionRoutePairs");
            builder.HasKey(pair => pair.Id);
            builder.Property(pair => pair.Id).ValueGeneratedNever();

            builder.HasIndex(pair => pair.AncillaryProvisionId);
        }
    }
}
