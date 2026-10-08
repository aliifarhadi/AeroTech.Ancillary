using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionSeatCharacteristicConfiguration : IEntityTypeConfiguration<ProvisionSeatCharacteristic>
    {
        public void Configure(EntityTypeBuilder<ProvisionSeatCharacteristic> builder)
        {
            builder.ToTable("ProvisionSeatCharacteristics");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.CharacteristicCode).HasMaxLength(25).IsRequired();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.CharacteristicCode }).IsUnique();
            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
