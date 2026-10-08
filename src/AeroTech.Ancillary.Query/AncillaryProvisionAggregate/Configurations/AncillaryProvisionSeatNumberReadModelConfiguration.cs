using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionSeatNumberReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionSeatNumberReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionSeatNumberReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionSeatNumbers");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.SeatNumber).HasMaxLength(16).IsRequired();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
