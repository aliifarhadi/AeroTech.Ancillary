using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionTravelDateReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionTravelDateReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionTravelDateReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionTravelDates");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
