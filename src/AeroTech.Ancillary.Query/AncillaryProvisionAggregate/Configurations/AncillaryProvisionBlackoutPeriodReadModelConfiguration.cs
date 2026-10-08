using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionBlackoutPeriodReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionBlackoutPeriodReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionBlackoutPeriodReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionBlackoutPeriods");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
