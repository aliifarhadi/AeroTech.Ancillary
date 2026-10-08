using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionDayTimeRestrictionReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionDayTimeRestrictionReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionDayTimeRestrictionReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionDayTimeRestrictions");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
