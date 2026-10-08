using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionSeatCharacteristicReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionSeatCharacteristicReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionSeatCharacteristicReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionSeatCharacteristics");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.CharacteristicCode).HasMaxLength(25).IsRequired();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
