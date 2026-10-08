using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionTravelDateConfiguration : IEntityTypeConfiguration<ProvisionTravelDate>
    {
        public void Configure(EntityTypeBuilder<ProvisionTravelDate> builder)
        {
            builder.ToTable("ProvisionTravelDates");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.TravelDate }).IsUnique();
        }
    }
}
