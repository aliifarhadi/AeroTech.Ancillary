using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionSeasonalPeriodReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionSeasonalPeriodReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionSeasonalPeriodReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionSeasonalPeriods");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
