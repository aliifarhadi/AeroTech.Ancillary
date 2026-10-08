using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionEligibleAgeBandReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionEligibleAgeBandReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionEligibleAgeBandReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionEligibleAgeBands");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
