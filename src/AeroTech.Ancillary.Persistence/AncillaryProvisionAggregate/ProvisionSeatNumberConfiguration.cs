using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionSeatNumberConfiguration : IEntityTypeConfiguration<ProvisionSeatNumber>
    {
        public void Configure(EntityTypeBuilder<ProvisionSeatNumber> builder)
        {
            builder.ToTable("ProvisionSeatNumbers");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.SeatNumber).HasMaxLength(16).IsRequired();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.SeatNumber }).IsUnique();
            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
